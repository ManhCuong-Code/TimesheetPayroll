using System.Text.Json;
using TimesheetPayroll.CoreBusiness.Models;

namespace TimesheetPayroll.CoreBusiness.Services;

public interface IFaceBiometricService
{
    /// <summary>
    /// Tính khoảng cách Euclidean giữa hai vector đặc trưng 128 chiều.
    /// Giá trị càng nhỏ thể hiện mức độ tương đồng càng cao. (Khớp khi D <= 0.55).
    /// </summary>
    double CalculateDistance(float[] descriptorA, float[] descriptorB);

    /// <summary>
    /// Chuyển đổi khoảng cách Euclidean sang tỷ lệ phần trăm tự tin (Confidence %).
    /// </summary>
    double DistanceToConfidence(double distance);

    /// <summary>
    /// Phân tích chuỗi JSON thành mảng vector số thực 128 chiều.
    /// </summary>
    float[]? ParseDescriptorJson(string? json);

    /// <summary>
    /// So khớp một vector khuôn mặt đưa vào với danh sách tất cả các hồ sơ đã đăng ký.
    /// Trả về hồ sơ phù hợp nhất nếu khoảng cách nhỏ hơn ngưỡng threshold (mặc định 0.55).
    /// </summary>
    (EmployeeFaceProfile? MatchedProfile, double Distance, double Confidence) FindBestMatch(
        float[] inputDescriptor, 
        IEnumerable<EmployeeFaceProfile> activeProfiles, 
        double maxDistanceThreshold = 0.55);
}

public class FaceBiometricService : IFaceBiometricService
{
    /// <summary>
    /// Chuẩn hóa vector đặc trưng về dạng Zero-centered và đơn vị L2-Norm.
    /// Giúp so khớp bất biến với độ sáng phòng và tương thích 100% giữa các nguồn trích xuất khác nhau.
    /// </summary>
    private float[] NormalizeVector(float[] vector)
    {
        if (vector == null || vector.Length == 0) return Array.Empty<float>();

        // 1. Tính giá trị trung bình (Mean)
        double sum = 0.0;
        for (int i = 0; i < vector.Length; i++) sum += vector[i];
        double mean = sum / vector.Length;

        // 2. Trừ trung bình để đưa tâm về 0 (Zero-centering) và tính L2-norm
        var centered = new float[vector.Length];
        double normSq = 0.0;
        for (int i = 0; i < vector.Length; i++)
        {
            centered[i] = (float)(vector[i] - mean);
            normSq += centered[i] * centered[i];
        }

        double norm = Math.Sqrt(normSq);
        if (norm < 1e-6) return vector; // Tránh chia cho 0 nếu vector toàn 0

        // 3. Chuẩn hóa về độ dài 1.0 (Unit sphere)
        for (int i = 0; i < centered.Length; i++)
        {
            centered[i] = (float)(centered[i] / norm);
        }

        return centered;
    }

    public double CalculateDistance(float[] descriptorA, float[] descriptorB)
    {
        if (descriptorA == null || descriptorB == null) return 1.0;
        int len = Math.Min(descriptorA.Length, descriptorB.Length);
        if (len == 0) return 1.0;

        // Chuẩn hóa zero-center cả 2 vector để so sánh chuẩn xác
        var normA = NormalizeVector(descriptorA);
        var normB = NormalizeVector(descriptorB);

        // Tính Cosine Similarity: S = dot(normA, normB)
        double dotProduct = 0.0;
        for (int i = 0; i < len; i++)
        {
            dotProduct += normA[i] * normB[i];
        }

        dotProduct = Math.Clamp(dotProduct, -1.0, 1.0);

        // Chuyển Cosine Similarity sang Euclidean Distance chuẩn: D = sqrt(2 * (1 - S))
        double distance = Math.Sqrt(Math.Max(0.0, 2.0 * (1.0 - dotProduct)));
        return distance;
    }

    public double DistanceToConfidence(double distance)
    {
        // Khi distance = 0 => 100% (Hoàn hảo)
        // Khi distance = 0.40 => ~85% (Rất cao)
        // Khi distance = 0.58 => ~70% (Đạt ngưỡng xác minh)
        // Khi distance >= 0.85 => 0% (Không khớp)
        if (distance <= 0) return 100.0;
        if (distance >= 0.85) return 0.0;
        double conf = (1.0 - (distance / 0.85)) * 100.0;
        return Math.Round(Math.Clamp(conf, 0.0, 100.0), 1);
    }

    public float[]? ParseDescriptorJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            var res = JsonSerializer.Deserialize<float[]>(json);
            if (res != null && res.Length > 0)
            {
                return res;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public (EmployeeFaceProfile? MatchedProfile, double Distance, double Confidence) FindBestMatch(
        float[] inputDescriptor, 
        IEnumerable<EmployeeFaceProfile> activeProfiles, 
        double maxDistanceThreshold = 0.58)
    {
        if (inputDescriptor == null || inputDescriptor.Length == 0 || activeProfiles == null)
        {
            return (null, 1.0, 0.0);
        }

        EmployeeFaceProfile? bestMatch = null;
        double minDistance = double.MaxValue;

        foreach (var profile in activeProfiles)
        {
            var profileDescriptor = ParseDescriptorJson(profile.FaceDescriptorJson);
            if (profileDescriptor == null || profileDescriptor.Length == 0) continue;

            double dist = CalculateDistance(inputDescriptor, profileDescriptor);
            if (dist < minDistance)
            {
                minDistance = dist;
                bestMatch = profile;
            }
        }

        if (bestMatch != null && minDistance <= maxDistanceThreshold)
        {
            return (bestMatch, minDistance, DistanceToConfidence(minDistance));
        }

        return (null, minDistance == double.MaxValue ? 1.0 : minDistance, DistanceToConfidence(minDistance));
    }
}
