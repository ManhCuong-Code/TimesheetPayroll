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
    public double CalculateDistance(float[] descriptorA, float[] descriptorB)
    {
        if (descriptorA == null || descriptorB == null) return 1.0;
        int len = Math.Min(descriptorA.Length, descriptorB.Length);
        if (len == 0) return 1.0;

        double sum = 0.0;
        for (int i = 0; i < len; i++)
        {
            double diff = descriptorA[i] - descriptorB[i];
            sum += diff * diff;
        }

        return Math.Sqrt(sum);
    }

    public double DistanceToConfidence(double distance)
    {
        // Khi distance = 0 => 100%
        // Khi distance = 0.55 (ngưỡng match) => ~70%
        // Khi distance >= 0.8 => 0%
        if (distance <= 0) return 100.0;
        if (distance >= 0.8) return 0.0;
        double conf = (1.0 - (distance / 0.80)) * 100.0;
        return Math.Round(Math.Clamp(conf, 0.0, 100.0), 1);
    }

    public float[]? ParseDescriptorJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<float[]>(json);
        }
        catch
        {
            return null;
        }
    }

    public (EmployeeFaceProfile? MatchedProfile, double Distance, double Confidence) FindBestMatch(
        float[] inputDescriptor, 
        IEnumerable<EmployeeFaceProfile> activeProfiles, 
        double maxDistanceThreshold = 0.55)
    {
        EmployeeFaceProfile? bestMatch = null;
        double minDistance = double.MaxValue;

        foreach (var profile in activeProfiles)
        {
            var profileDescriptor = ParseDescriptorJson(profile.FaceDescriptorJson);
            if (profileDescriptor == null) continue;

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

        return (null, minDistance == double.MaxValue ? 1.0 : minDistance, 0.0);
    }
}
