using System.Collections.Concurrent;
using System.Text.Json;
using TimesheetPayroll.CoreBusiness.Models;

namespace TimesheetPayroll.CoreBusiness.Services;

public interface IFaceBiometricService
{
    /// <summary>
    /// Tính khoảng cách Euclidean giữa hai vector đặc trưng 128 chiều.
    /// Giá trị càng nhỏ thể hiện mức độ tương đồng càng cao. (Khớp khi D <= 0.58).
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
    /// Kiểm tra tính hợp lệ toán học và chuẩn hóa Zero-Centered + L2-Norm cho vector 128-D.
    /// </summary>
    bool ValidateAndNormalizeDescriptor(float[]? descriptor, out float[] normalized, out string? errorMessage);

    /// <summary>
    /// So khớp một vector khuôn mặt đưa vào với danh sách tất cả các hồ sơ đã đăng ký.
    /// Trả về hồ sơ phù hợp nhất nếu khoảng cách nhỏ hơn ngưỡng threshold (mặc định 0.58).
    /// </summary>
    (EmployeeFaceProfile? MatchedProfile, double Distance, double Confidence) FindBestMatch(
        float[] inputDescriptor, 
        IEnumerable<EmployeeFaceProfile> activeProfiles, 
        double maxDistanceThreshold = 0.58);

    /// <summary>
    /// Nạp trước danh sách hồ sơ khuôn mặt từ cơ sở dữ liệu bền vững vào bộ nhớ đệm L2-Normalized khi khởi động server.
    /// </summary>
    int PreloadProfiles(IEnumerable<EmployeeFaceProfile>? profiles);

    /// <summary>
    /// Làm mới bộ nhớ đệm vector đã chuẩn hóa khi có thay đổi hồ sơ khuôn mặt.
    /// </summary>
    void InvalidateCache(long? employeeId = null);
}

public class FaceBiometricService : IFaceBiometricService
{
    public const int ExpectedDimension = 128;

    private sealed record CachedEmbeddingEntry(int JsonHashCode, DateTime UpdatedAt, float[] NormalizedVector);

    // Bộ nhớ đệm thread-safe lưu vector 128-D đã parse & chuẩn hóa L2 theo EmployeeId, tránh parse JSON lặp lại mỗi lần quét
    private readonly ConcurrentDictionary<long, CachedEmbeddingEntry> _normalizedVectorCache = new();

    /// <summary>
    /// Chuẩn hóa vector đặc trưng về dạng Zero-centered và đơn vị L2-Norm.
    /// Giúp so khớp bất biến với độ sáng phòng và tương thích 100% giữa các nguồn trích xuất khác nhau.
    /// </summary>
    private float[] NormalizeVector(float[] vector)
    {
        if (vector == null || vector.Length == 0) return Array.Empty<float>();

        int len = vector.Length;
        double sum = 0.0;
        for (int i = 0; i < len; i++)
        {
            sum += vector[i];
        }
        double mean = sum / len;

        var centered = new float[len];
        double normSq = 0.0;
        for (int i = 0; i < len; i++)
        {
            double c = vector[i] - mean;
            centered[i] = (float)c;
            normSq += c * c;
        }

        double norm = Math.Sqrt(normSq);
        if (norm < 1e-6) return vector;

        double invNorm = 1.0 / norm;
        for (int i = 0; i < len; i++)
        {
            centered[i] = (float)(centered[i] * invNorm);
        }

        return centered;
    }

    public bool ValidateAndNormalizeDescriptor(float[]? descriptor, out float[] normalized, out string? errorMessage)
    {
        normalized = Array.Empty<float>();
        errorMessage = null;

        if (descriptor == null || descriptor.Length != ExpectedDimension)
        {
            errorMessage = $"Vector đặc trưng khuôn mặt phải có đúng {ExpectedDimension} chiều (nhận được {descriptor?.Length ?? 0} chiều).";
            return false;
        }

        double sum = 0.0;
        for (int i = 0; i < ExpectedDimension; i++)
        {
            float v = descriptor[i];
            if (!float.IsFinite(v))
            {
                errorMessage = "Vector đặc trưng khuôn mặt chứa giá trị không hợp lệ (NaN hoặc Infinity).";
                return false;
            }
            sum += v;
        }

        double mean = sum / ExpectedDimension;
        var result = new float[ExpectedDimension];
        double normSq = 0.0;

        for (int i = 0; i < ExpectedDimension; i++)
        {
            double c = descriptor[i] - mean;
            normSq += c * c;
            result[i] = (float)c;
        }

        double norm = Math.Sqrt(normSq);
        if (norm < 1e-5)
        {
            errorMessage = "Ảnh khuôn mặt không đủ độ tương phản hoặc đồng nhất bất thường (vector phương sai bằng 0). Vui lòng kiểm tra camera và ánh sáng.";
            return false;
        }

        double invNorm = 1.0 / norm;
        for (int i = 0; i < ExpectedDimension; i++)
        {
            result[i] = MathF.Round((float)(result[i] * invNorm), 6);
        }

        normalized = result;
        return true;
    }

    private static double ComputeUnitVectorDistance(float[] unitA, float[] unitB)
    {
        int len = Math.Min(unitA.Length, unitB.Length);
        if (len == 0) return 1.0;

        double dotProduct = 0.0;
        for (int i = 0; i < len; i++)
        {
            dotProduct += unitA[i] * unitB[i];
        }

        dotProduct = Math.Clamp(dotProduct, -1.0, 1.0);
        return Math.Sqrt(Math.Max(0.0, 2.0 * (1.0 - dotProduct)));
    }

    public double CalculateDistance(float[] descriptorA, float[] descriptorB)
    {
        if (descriptorA == null || descriptorB == null) return 1.0;
        int len = Math.Min(descriptorA.Length, descriptorB.Length);
        if (len == 0) return 1.0;

        var normA = NormalizeVector(descriptorA);
        var normB = NormalizeVector(descriptorB);

        return ComputeUnitVectorDistance(normA, normB);
    }

    public double DistanceToConfidence(double distance)
    {
        if (distance <= 0) return 100.0;
        if (distance >= 0.85) return 0.0;
        double conf;
        if (distance <= 0.58)
        {
            conf = 100.0 - (distance / 0.58) * 30.0;
        }
        else
        {
            conf = 70.0 * (1.0 - (distance - 0.58) / (0.85 - 0.58));
        }
        return Math.Round(Math.Clamp(conf, 0.0, 100.0), 1);
    }

    public float[]? ParseDescriptorJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]" || json == "[ENROLLED]") return null;
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

    private float[]? GetOrParseNormalizedProfileVector(EmployeeFaceProfile profile)
    {
        if (string.IsNullOrWhiteSpace(profile.FaceDescriptorJson) || profile.FaceDescriptorJson == "[]" || profile.FaceDescriptorJson == "[ENROLLED]")
        {
            return null;
        }

        int jsonHash = profile.FaceDescriptorJson.GetHashCode(StringComparison.Ordinal);
        if (profile.EmployeeId > 0 &&
            _normalizedVectorCache.TryGetValue(profile.EmployeeId, out var cached) &&
            cached.JsonHashCode == jsonHash &&
            cached.UpdatedAt == profile.UpdatedAt)
        {
            return cached.NormalizedVector;
        }

        var parsed = ParseDescriptorJson(profile.FaceDescriptorJson);
        if (parsed == null || parsed.Length != ExpectedDimension)
        {
            return null;
        }

        if (!ValidateAndNormalizeDescriptor(parsed, out var normalized, out _))
        {
            return null;
        }

        if (profile.EmployeeId > 0)
        {
            _normalizedVectorCache[profile.EmployeeId] = new CachedEmbeddingEntry(jsonHash, profile.UpdatedAt, normalized);
        }

        return normalized;
    }

    public int PreloadProfiles(IEnumerable<EmployeeFaceProfile>? profiles)
    {
        if (profiles == null) return 0;
        int loadedCount = 0;
        foreach (var profile in profiles)
        {
            var vec = GetOrParseNormalizedProfileVector(profile);
            if (vec != null && vec.Length == ExpectedDimension)
            {
                loadedCount++;
            }
        }
        return loadedCount;
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

        var normInput = NormalizeVector(inputDescriptor);
        if (normInput.Length == 0)
        {
            return (null, 1.0, 0.0);
        }

        EmployeeFaceProfile? bestMatch = null;
        double minDistance = double.MaxValue;

        foreach (var profile in activeProfiles)
        {
            var normProfile = GetOrParseNormalizedProfileVector(profile);
            if (normProfile == null || normProfile.Length == 0) continue;

            double dist = ComputeUnitVectorDistance(normInput, normProfile);
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

    public void InvalidateCache(long? employeeId = null)
    {
        if (employeeId.HasValue)
        {
            _normalizedVectorCache.TryRemove(employeeId.Value, out _);
        }
        else
        {
            _normalizedVectorCache.Clear();
        }
    }
}
