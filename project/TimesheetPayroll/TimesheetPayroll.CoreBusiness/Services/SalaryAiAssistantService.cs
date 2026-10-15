using System.Globalization;
using System.Text;
using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;

namespace TimesheetPayroll.CoreBusiness.Services;

/// <summary>
/// Triển khai dịch vụ Trợ lý Lương AI thông minh (Salary AI Assistant Service).
/// Tích hợp động cơ phân tích đối soát dữ liệu thực tế (Grounded Reasoning Engine) và tính toán mô phỏng What-If.
/// </summary>
public class SalaryAiAssistantService : ISalaryAiAssistantService
{
    private readonly ISalaryFormulaEngine _formulaEngine;
    private readonly ILlmClient? _llmClient;
    private readonly IAiKnowledgeBaseService? _knowledgeBaseService;
    private static readonly CultureInfo VnCulture = new("vi-VN");

    public SalaryAiAssistantService(
        ISalaryFormulaEngine formulaEngine,
        ILlmClient? llmClient = null,
        IAiKnowledgeBaseService? knowledgeBaseService = null)
    {
        _formulaEngine = formulaEngine;
        _llmClient = llmClient;
        _knowledgeBaseService = knowledgeBaseService;
    }

    /// <summary>
    /// Đối soát và phân tích sự chênh lệch chi tiết giữa hai phiếu lương của người lao động.
    /// </summary>
    public SalaryComparisonDiff ComparePayslips(Payslip currentPeriod, Payslip previousPeriod, Employee? employee = null)
    {
        var diff = new SalaryComparisonDiff
        {
            PeriodA = currentPeriod,
            PeriodB = previousPeriod
        };

        // 1. Phân tích từng dòng chi tiết (Line Diff)
        var linesA = currentPeriod.Lines ?? new List<PayslipLine>();
        var linesB = previousPeriod.Lines ?? new List<PayslipLine>();

        // Tập hợp tất cả các loại khoản mục xuất hiện ở cả 2 kỳ
        var allLineTypes = linesA.Select(l => l.LineType)
            .Union(linesB.Select(l => l.LineType))
            .Distinct()
            .ToList();

        foreach (var type in allLineTypes)
        {
            var itemA = linesA.FirstOrDefault(l => l.LineType == type);
            var itemB = linesB.FirstOrDefault(l => l.LineType == type);

            decimal amountA = itemA?.Amount ?? 0m;
            decimal amountB = itemB?.Amount ?? 0m;
            bool isDeduction = itemA?.IsDeduction ?? itemB?.IsDeduction ?? false;
            string description = itemA?.LineDescription ?? itemB?.LineDescription ?? type.ToString();

            string reason = GetLineDiffReason(type, amountA, amountB, currentPeriod, previousPeriod);

            diff.LineDiffs.Add(new SalaryLineDiffItem
            {
                Title = description,
                Category = isDeduction ? "Khấu trừ" : "Thu nhập",
                AmountA = amountA,
                AmountB = amountB,
                Reason = reason
            });
        }

        // 2. Xác định các nhân tố chính (Key Factors) gây biến động lương
        IdentifyKeyFactors(diff, currentPeriod, previousPeriod);

        // 3. Tạo văn bản tóm tắt tự nhiên từ Trợ lý AI (Grounded Prompt Output)
        diff.AiSummary = GenerateAiSummary(diff, currentPeriod, previousPeriod, employee);

        return diff;
    }

    /// <summary>
    /// Trả lời câu hỏi tự nhiên của nhân viên về lương.
    /// </summary>
    public async Task<AiChatMessage> AnswerQuestionAsync(
        string question, 
        Payslip? current, 
        Payslip? previous, 
        Employee? employee, 
        IEnumerable<PayrollPeriod>? allPeriods = null)
    {
        SalaryComparisonDiff? diff = null;
        if (current != null && previous != null)
        {
            diff = ComparePayslips(current, previous, employee);
        }

        // 1. ƯU TIÊN MÔ HÌNH LLM (Google Gemini API / OpenAI API) KHI ĐÃ ĐƯỢC CẤU HÌNH
        if (_llmClient != null && _llmClient.IsConfigured && _knowledgeBaseService != null)
        {
            try
            {
                string systemPrompt = _knowledgeBaseService.BuildFullSystemPrompt(employee, current, previous, diff);
                string? llmAnswer = await _llmClient.GenerateResponseAsync(systemPrompt, question);
                if (!string.IsNullOrWhiteSpace(llmAnswer))
                {
                    return new AiChatMessage
                    {
                        IsUser = false,
                        Timestamp = DateTime.Now,
                        Text = llmAnswer.Trim(),
                        ComparisonDiff = diff,
                        ProviderUsed = $"{_llmClient.ProviderName} ({_llmClient.ActiveModel})",
                        IsLlmGenerated = true,
                        FollowUpSuggestions = GenerateFollowUpSuggestions(question)
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SalaryAiAssistantService] Lỗi khi gọi LLM: {ex.Message}");
            }
        }

        // 2. FALLBACK VỀ ĐỘNG CƠ PHÂN TÍCH NỘI BỘ (Local Grounded Engine)
        var localMsg = AnswerQuestionInternal(question, current, previous, employee, diff);
        localMsg.ProviderUsed = "Động cơ Phân tích Nội bộ (Local Grounded Engine)";
        localMsg.IsLlmGenerated = false;
        return localMsg;
    }

    private AiChatMessage AnswerQuestionInternal(
        string question, 
        Payslip? current, 
        Payslip? previous, 
        Employee? employee,
        SalaryComparisonDiff? diff)
    {
        string qLower = (question ?? "").Trim().ToLowerInvariant();
        var response = new AiChatMessage
        {
            IsUser = false,
            Timestamp = DateTime.Now
        };

        // Kịch bản 1: Hỏi về so sánh / chênh lệch lương
        if (qLower.Contains("ít hơn") || qLower.Contains("thấp hơn") || qLower.Contains("giảm") || 
            qLower.Contains("so sánh") || qLower.Contains("chênh lệch") || qLower.Contains("tháng trước") ||
            qLower.Contains("tại sao") || qLower.Contains("tháng 9") || qLower.Contains("tháng 10") ||
            qLower.Contains("thay đổi"))
        {
            if (current != null && previous != null)
            {
                diff ??= ComparePayslips(current, previous, employee);
                response.ComparisonDiff = diff;
                response.Text = diff.AiSummary;
                response.FollowUpSuggestions = new List<string>
                {
                    "Giải thích các khoản bảo hiểm bắt buộc",
                    "Cách tính thuế thu nhập cá nhân (TNCN)",
                    "Mô phỏng lương nếu tháng tới làm đủ 22 công và 10h OT"
                };
                return response;
            }
            else if (current != null)
            {
                response.Text = $"Chào bạn! Hiện tại hệ thống ghi nhận phiếu lương gần nhất là **Tháng {current.PeriodMonth}/{current.PeriodYear}** với thực lĩnh **{current.NetSalary.ToString("N0", VnCulture)} đ**. Chưa tìm thấy dữ liệu kỳ lương trước đó để thực hiện so sánh đối soát tự động.";
                response.FollowUpSuggestions = new List<string>
                {
                    "Xem chi tiết phiếu lương hiện tại",
                    "Công thức tính tiền làm thêm giờ (OT)"
                };
                return response;
            }
        }

        // Kịch bản 2: Hỏi về bảo hiểm bắt buộc (BHXH, BHYT, BHTN)
        if (qLower.Contains("bảo hiểm") || qLower.Contains("bhxh") || qLower.Contains("bhyt") || qLower.Contains("bhtn") || qLower.Contains("10.5"))
        {
            var sb = new StringBuilder();
            sb.AppendLine("🛡️ **Quy định trích nộp Bảo hiểm bắt buộc theo các Nghị định mới nhất của Chính phủ:**\n");
            sb.AppendLine("Tổng tỷ lệ người lao động đóng góp là **10.5%** trên mức lương đóng bảo hiểm, gồm:");
            sb.AppendLine("• **Bảo hiểm Xã hội (BHXH): 8.0%** — Chi trả các chế độ ốm đau, thai sản, tai nạn lao động, hưu trí và tử tuất.");
            sb.AppendLine("• **Bảo hiểm Y tế (BHYT): 1.5%** — Hỗ trợ chi phí khám chữa bệnh và chăm sóc sức khỏe.");
            sb.AppendLine("• **Bảo hiểm Thất nghiệp (BHTN): 1.0%** — Hưởng trợ cấp thất nghiệp khi chấm dứt hợp đồng.");
            sb.AppendLine("\n📌 **Các căn cứ pháp lý & mức trần mới nhất:**");
            sb.AppendLine($"• **Nghị định 73/2024/NĐ-CP (từ 01/07/2024):** Mức lương cơ sở là **{VietnameseLaborRegulations.BaseStatutorySalary:N0} đ/tháng**. Mức trần tiền lương đóng BHXH và BHYT tối đa bằng 20 lần mức lương cơ sở = **{VietnameseLaborRegulations.MaxSalaryCapBhxhBhyt:N0} đ/tháng**.");
            sb.AppendLine($"• **Nghị định 74/2024/NĐ-CP (từ 01/07/2024):** Mức trần tiền lương đóng BHTN tối đa bằng 20 lần lương tối thiểu vùng I ({VietnameseLaborRegulations.RegionalMinimumSalaryRegion1:N0} đ) = **{VietnameseLaborRegulations.MaxSalaryCapBhtn:N0} đ/tháng**.");
            sb.AppendLine($"• **Điều 85 Luật BHXH & QĐ 595/QĐ-BHXH:** Nếu người lao động không làm việc và không hưởng tiền lương từ **{VietnameseLaborRegulations.UnpaidLeaveDaysThresholdForInsuranceExemption} ngày làm việc trở lên** trong tháng thì tháng đó không phải đóng bảo hiểm.");
            sb.AppendLine($"\n*(Ngoài ra, Doanh nghiệp đóng thêm {VietnameseLaborRegulations.EmployerTotalInsuranceRate * 100:0.#}% cho bạn, nâng tổng mức đóng toàn bộ lên 32%).*");

            if (current != null)
            {
                var bhxh = current.Lines.FirstOrDefault(l => l.LineType == PayslipLineType.DEDUCTION_BHXH)?.Amount ?? 0m;
                var bhyt = current.Lines.FirstOrDefault(l => l.LineType == PayslipLineType.DEDUCTION_BHYT)?.Amount ?? 0m;
                var bhtn = current.Lines.FirstOrDefault(l => l.LineType == PayslipLineType.DEDUCTION_BHTN)?.Amount ?? 0m;
                if (bhxh + bhyt + bhtn > 0)
                {
                    sb.AppendLine($"\n👉 **Thực tế trên phiếu lương Tháng {current.PeriodMonth}/{current.PeriodYear} của bạn:**");
                    sb.AppendLine($"• BHXH (8%): **{bhxh.ToString("N0", VnCulture)} đ**");
                    sb.AppendLine($"• BHYT (1.5%): **{bhyt.ToString("N0", VnCulture)} đ**");
                    sb.AppendLine($"• BHTN (1%): **{bhtn.ToString("N0", VnCulture)} đ**");
                    sb.AppendLine($"=> Tổng trích bảo hiểm: **{(bhxh + bhyt + bhtn).ToString("N0", VnCulture)} đ**.");
                }
            }

            response.Text = sb.ToString();
            response.FollowUpSuggestions = new List<string>
            {
                "Cách tính thuế thu nhập cá nhân (TNCN)",
                "Quy định về ngày nghỉ phép năm có lương",
                "Mô phỏng thử lương"
            };
            return response;
        }

        // Kịch bản 3: Hỏi về thuế thu nhập cá nhân (TNCN) & giảm trừ gia cảnh
        if (qLower.Contains("thuế") || qLower.Contains("tncn") || qLower.Contains("giảm trừ") || qLower.Contains("gia cảnh") || qLower.Contains("người phụ thuộc"))
        {
            var sb = new StringBuilder();
            sb.AppendLine("📋 **Quy định tính Thuế Thu nhập Cá nhân (TNCN) mới nhất:**\n");
            sb.AppendLine($"1. **Thu nhập chịu thuế** = Tổng thu nhập Gross - Các khoản phụ cấp được miễn thuế (tiền ăn trưa tối đa {VietnameseLaborRegulations.MaxNonTaxableMealAllowance:N0} đ/tháng, phần thặng dư làm thêm giờ OT).");
            sb.AppendLine("2. **Các khoản giảm trừ hợp lệ:**");
            sb.AppendLine($"   • **Giảm trừ bản thân:** **{VietnameseLaborRegulations.PersonalTaxDeduction:N0} đ/tháng** (mức mới nhất: thu nhập từ 15 triệu đồng mới bắt đầu tính thuế TNCN).");
            sb.AppendLine($"   • **Giảm trừ người phụ thuộc:** **{VietnameseLaborRegulations.DependentTaxDeduction:N0} đ/người/tháng** theo Nghị quyết 954/2020/UBTVQH14.");
            sb.AppendLine("   • Toàn bộ 10.5% các khoản bảo hiểm bắt buộc theo luật (BHXH, BHYT, BHTN).");
            sb.AppendLine("3. **Thu nhập tính thuế** = Thu nhập chịu thuế - Các khoản giảm trừ (chỉ đóng thuế khi số này > 0).");
            sb.AppendLine("4. Áp dụng **Biểu thuế lũy tiến từng phần:**");
            sb.AppendLine("   • Bậc 1 (đến 5 triệu đ): 5%");
            sb.AppendLine("   • Bậc 2 (từ 5 đến 10 triệu đ): 10% trừ 250.000 đ");
            sb.AppendLine("   • Bậc 3 (từ 10 đến 18 triệu đ): 15% trừ 750.000 đ");
            sb.AppendLine("   • Bậc 4 (từ 18 đến 32 triệu đ): 20% trừ 1.650.000 đ...");

            response.Text = sb.ToString();
            response.FollowUpSuggestions = new List<string>
            {
                "Tính thử thuế TNCN với 1 người phụ thuộc",
                "Giải thích các khoản bảo hiểm bắt buộc",
                "Sao lương tháng này ít hơn tháng trước?"
            };
            return response;
        }

        // Kịch bản 4: Hỏi về nghỉ phép năm, nghỉ lễ tết, nghỉ việc riêng có lương
        if (qLower.Contains("phép") || qLower.Contains("nghỉ phép") || qLower.Contains("nghỉ lễ") || qLower.Contains("kết hôn") || qLower.Contains("nghỉ việc riêng"))
        {
            var sb = new StringBuilder();
            sb.AppendLine("🌴 **Quy định Chế độ Nghỉ phép và Nghỉ Lễ, Tết theo Bộ luật Lao động 2019:**\n");
            sb.AppendLine($"1. **Nghỉ phép năm hưởng nguyên lương (Điều 113):**");
            sb.AppendLine($"   • Người lao động làm việc đủ 12 tháng được nghỉ **{VietnameseLaborRegulations.BasicAnnualLeaveDaysPerYear} ngày phép năm** hưởng nguyên 100% lương.");
            sb.AppendLine($"   • Cứ mỗi **{VietnameseLaborRegulations.SeniorityYearsPerBonusLeaveDay} năm thâm niên** làm việc thì số ngày phép năm tăng thêm **1 ngày** (Điều 114).");
            sb.AppendLine($"2. **Nghỉ Lễ, Tết hưởng nguyên lương (Điều 112):** Tổng cộng **{VietnameseLaborRegulations.TotalPaidPublicHolidays} ngày/năm** (Tết Dương lịch 1 ngày, Tết Âm lịch 5 ngày, Giỗ tổ Hùng Vương 1 ngày, 30/4 & 1/5 2 ngày, Quốc khánh 2/9 2 ngày).");
            sb.AppendLine("3. **Nghỉ việc riêng có hưởng lương (Điều 115):**");
            sb.AppendLine($"   • Bản thân kết hôn: Nghỉ **{VietnameseLaborRegulations.MarriageLeaveDays} ngày**.");
            sb.AppendLine($"   • Con đẻ/con nuôi kết hôn: Nghỉ **{VietnameseLaborRegulations.ChildMarriageLeaveDays} ngày**.");
            sb.AppendLine($"   • Cha mẹ đẻ, cha mẹ vợ/chồng, vợ/chồng, con chết: Nghỉ **{VietnameseLaborRegulations.BereavementLeaveDays} ngày**.");

            response.Text = sb.ToString();
            response.FollowUpSuggestions = new List<string>
            {
                "Quy định làm việc vào ban đêm và tăng ca",
                "Giải thích các khoản bảo hiểm bắt buộc",
                "Mô phỏng lương tháng tới"
            };
            return response;
        }

        // Kịch bản 5: Hỏi về tăng ca / Overtime (OT) và làm ban đêm
        if (qLower.Contains("ot") || qLower.Contains("tăng ca") || qLower.Contains("làm thêm") || qLower.Contains("ngoài giờ") || qLower.Contains("ca đêm") || qLower.Contains("ban đêm"))
        {
            var sb = new StringBuilder();
            sb.AppendLine("⏰ **Quy chế tính tiền Làm thêm giờ (Overtime) & Làm đêm (Điều 98 Bộ luật Lao động 2019):**\n");
            sb.AppendLine("• **Đơn giá 1 giờ chuẩn** = Lương cơ bản / (Ngày công chuẩn trong tháng × 8 giờ).");
            sb.AppendLine("• **Hệ số hưởng lương làm thêm:**");
            sb.AppendLine("   - Ngày làm việc bình thường: Hưởng **150%** đơn giá giờ.");
            sb.AppendLine("   - Ngày nghỉ hàng tuần (Thứ 7, Chủ Nhật): Hưởng **200%** đơn giá giờ.");
            sb.AppendLine("   - Ngày nghỉ Lễ, Tết: Hưởng **300%** đơn giá giờ.");
            sb.AppendLine($"• **Làm việc vào ban đêm (22h - 06h):** Được trả thêm ít nhất **{VietnameseLaborRegulations.NightShiftBonusRate * 100:0.#}%** tiền lương giờ.");
            sb.AppendLine($"• **Làm thêm giờ ban đêm:** Được trả thêm ít nhất **{VietnameseLaborRegulations.NightOvertimeBonusRate * 100:0.#}%** tiền lương tính theo công việc làm thêm vào ban ngày.");
            sb.AppendLine("\n💡 *Điểm i Khoản 1 Điều 3 TT 111/2013: Phần tiền lương trả cao hơn do làm thêm giờ được miễn thuế TNCN.*");

            response.Text = sb.ToString();
            response.FollowUpSuggestions = new List<string>
            {
                "Mô phỏng lương làm thêm 10 giờ OT",
                "Chi tiết phiếu lương tháng này"
            };
            return response;
        }

        // Kịch bản 6: Hỏi về thử việc
        if (qLower.Contains("thử việc") || qLower.Contains("probation"))
        {
            var sb = new StringBuilder();
            sb.AppendLine("📝 **Quy định Tiền lương Thử việc (Điều 26 Bộ luật Lao động 2019):**\n");
            sb.AppendLine($"• Tiền lương của người lao động trong thời gian thử việc do hai bên thỏa thuận nhưng ít nhất phải bằng **{VietnameseLaborRegulations.ProbationSalaryMinimumRate * 100:0.#}%** mức lương của công việc đó.");
            sb.AppendLine("• Trong thời gian thử việc, người lao động chưa thuộc đối tượng tham gia BHXH bắt buộc nếu ký hợp đồng thử việc riêng biệt.");

            response.Text = sb.ToString();
            response.FollowUpSuggestions = new List<string>
            {
                "Giải thích các khoản bảo hiểm bắt buộc",
                "Cách tính thuế thu nhập cá nhân (TNCN)"
            };
            return response;
        }

        // Kịch bản 7: Tra cứu chi tiết phiếu lương hiện tại
        if (qLower.Contains("phiếu lương") || qLower.Contains("thực lĩnh") || qLower.Contains("lương của tôi") || qLower.Contains("lương tháng này"))
        {
            if (current != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"📄 **Tóm tắt Phiếu lương Tháng {current.PeriodMonth}/{current.PeriodYear} của {current.EmployeeName}:**\n");
                sb.AppendLine($"• Mã nhân viên: **{current.EmployeeCode}**");
                sb.AppendLine($"• Số ngày công thực tế: **{current.ActualWorkDays} công**");
                sb.AppendLine($"• Tổng thu nhập gộp (Gross): **{current.GrossSalary.ToString("N0", VnCulture)} đ**");
                sb.AppendLine($"• Tổng các khoản khấu trừ: **{current.TotalDeductions.ToString("N0", VnCulture)} đ**");
                sb.AppendLine($"• **Thực lĩnh chuyển khoản (Net): {current.NetSalary.ToString("N0", VnCulture)} đ**");
                sb.AppendLine($"• Tài khoản nhận: **{current.BankAccountNumber}** ({current.BankName})");

                response.Text = sb.ToString();
                response.FollowUpSuggestions = new List<string>
                {
                    "So sánh với tháng trước",
                    "Giải thích các khoản bảo hiểm",
                    "Mô phỏng thử lương"
                };
                return response;
            }
        }

        // Kịch bản 8: Câu chào hỏi hoặc hỏi chung chung
        var defaultSb = new StringBuilder();
        string empName = employee?.FullName ?? current?.EmployeeName ?? "bạn";
        defaultSb.AppendLine($"Dạ chào anh/chị **{empName}**! Em là **Trợ lý Lương AI** thông minh của doanh nghiệp.");
        defaultSb.AppendLine("Em có thể hỗ trợ anh/chị:");
        defaultSb.AppendLine("1. **Giải thích biến động lương:** Phân tích nguyên nhân vì sao lương tháng này tăng hoặc giảm so với tháng trước.");
        defaultSb.AppendLine("2. **Tra cứu luật & chế độ:** Giải thích bảo hiểm (10.5%), thuế TNCN, chế độ nghỉ phép năm, làm đêm, làm thêm giờ (OT).");
        defaultSb.AppendLine("3. **Mô phỏng What-If:** Dự tính số tiền thực lĩnh nếu tháng tới làm thêm giờ hoặc nghỉ phép.");
        defaultSb.AppendLine("\nAnh/chị có thể bấm chọn các gợi ý bên dưới hoặc gõ câu hỏi trực tiếp nhé!");

        response.Text = defaultSb.ToString();
        response.FollowUpSuggestions = new List<string>
        {
            "Sao lương tháng này ít hơn tháng trước?",
            "Giải thích các khoản bảo hiểm bắt buộc",
            "Quy định về ngày nghỉ phép năm có lương",
            "Cách tính thuế thu nhập cá nhân (TNCN)",
            "Công thức tính tiền làm thêm giờ (OT)"
        };
        return response;
    }

    private static List<string> GenerateFollowUpSuggestions(string question)
    {
        string q = (question ?? "").ToLowerInvariant();
        if (q.Contains("giảm") || q.Contains("ít hơn") || q.Contains("so sánh"))
        {
            return new List<string>
            {
                "Giải thích các khoản bảo hiểm bắt buộc",
                "Cách tính thuế thu nhập cá nhân (TNCN)",
                "Mô phỏng lương tháng tới"
            };
        }
        if (q.Contains("bảo hiểm") || q.Contains("bhxh"))
        {
            return new List<string>
            {
                "Cách tính thuế thu nhập cá nhân (TNCN)",
                "Quy định 14 ngày nghỉ không lương",
                "Chi tiết phiếu lương tháng này"
            };
        }
        return new List<string>
        {
            "So sánh với tháng trước",
            "Quy định về ngày nghỉ phép năm có lương",
            "Mô phỏng What-If tính thử lương"
        };
    }

    /// <summary>
    /// Mô phỏng dự tính lương thực lĩnh dựa trên các tham số giả định của người dùng (What-If Salary Simulator).
    /// </summary>
    public SalarySimulationResult SimulateSalary(SalarySimulationRequest request, IEnumerable<DeductionRate>? rates = null)
    {
        var result = new SalarySimulationResult
        {
            BaseSalary = request.BaseSalary,
            StandardWorkDays = request.StandardWorkDays > 0 ? request.StandardWorkDays : 22m,
            ActualWorkDays = request.ActualWorkDays,
            PaidLeavePay = 0m,
            OvertimeHours = request.OvertimeHours,
            Allowances = request.MealAllowance + request.FuelAllowance,
            Bonus = request.Bonus
        };

        // 1. Lương công thực tế
        result.ProratedBaseSalary = _formulaEngine.ComputeProratedBaseSalary(
            result.BaseSalary, 
            result.ActualWorkDays, 
            result.StandardWorkDays);

        // 2. Lương ngày phép năm có lương
        if (request.PaidLeaveDays > 0)
        {
            result.PaidLeavePay = _formulaEngine.ComputePaidLeavePay(
                result.BaseSalary, 
                request.PaidLeaveDays, 
                result.StandardWorkDays);
        }

        // 3. Tiền làm thêm giờ (OT)
        if (request.OvertimeHours > 0)
        {
            result.OvertimePay = _formulaEngine.ComputeOvertimePay(
                result.BaseSalary, 
                result.StandardWorkDays, 
                request.OvertimeHours, 
                request.OvertimeMultiplier);
        }

        // 4. Tổng Gross
        result.GrossSalary = _formulaEngine.ComputeGrossSalary(
            result.ProratedBaseSalary, 
            result.PaidLeavePay, 
            result.OvertimePay, 
            result.Allowances, 
            result.Bonus);

        // 5. Bảo hiểm bắt buộc theo Nghị định 73/2024/NĐ-CP (trần 46.8tr) & Nghị định 74/2024/NĐ-CP (trần 99.2tr)
        // Kiểm tra quy tắc 14 ngày nghỉ không lương (Điều 85 Luật BHXH 2014)
        decimal totalPaid = result.ActualWorkDays + (request.PaidLeaveDays > 0 ? request.PaidLeaveDays : 0m);
        if (result.StandardWorkDays - totalPaid >= 14m)
        {
            result.Bhxh = 0m;
            result.Bhyt = 0m;
            result.Bhtn = 0m;
            result.TotalInsurance = 0m;
        }
        else
        {
            decimal capBhxh = Math.Min(result.BaseSalary, VietnameseLaborRegulations.MaxSalaryCapBhxhBhyt);
            decimal capBhtn = Math.Min(result.BaseSalary, VietnameseLaborRegulations.MaxSalaryCapBhtn);
            result.Bhxh = Math.Round(capBhxh * 0.08m, 0, MidpointRounding.AwayFromZero);
            result.Bhyt = Math.Round(capBhxh * 0.015m, 0, MidpointRounding.AwayFromZero);
            result.Bhtn = Math.Round(capBhtn * 0.01m, 0, MidpointRounding.AwayFromZero);
            result.TotalInsurance = result.Bhxh + result.Bhyt + result.Bhtn;
        }

        // 6. Giảm trừ gia cảnh theo Nghị quyết 954/2020: Bản thân 11tr, Người phụ thuộc 4.4tr/người
        result.PersonalDeduction = VietnameseLaborRegulations.PersonalTaxDeduction;
        result.DependentDeduction = request.NumberOfDependents * VietnameseLaborRegulations.DependentTaxDeduction;

        // Phụ cấp ăn trưa miễn thuế tối đa 730k (Thông tư 26/2016)
        decimal nonTaxableMeal = Math.Min(request.MealAllowance, VietnameseLaborRegulations.MaxNonTaxableMealAllowance);

        // Phần tiền làm thêm giờ (OT) cao hơn mức tiêu chuẩn được miễn thuế TNCN (Điểm i Khoản 1 Điều 3 TT 111/2013)
        decimal nonTaxableOt = 0m;
        if (result.OvertimePay > 0 && request.OvertimeMultiplier > 1.0m)
        {
            nonTaxableOt = Math.Round(result.OvertimePay * ((request.OvertimeMultiplier - 1.0m) / request.OvertimeMultiplier), 0, MidpointRounding.AwayFromZero);
        }
        decimal totalNonTaxable = nonTaxableMeal + nonTaxableOt;

        result.TaxableIncome = Math.Max(0m, result.GrossSalary - totalNonTaxable);

        // Thu nhập tính thuế = Chịu thuế - Bảo hiểm - Bản thân - Người phụ thuộc
        result.AssessableIncome = Math.Max(0m, result.TaxableIncome - result.TotalInsurance - result.PersonalDeduction - result.DependentDeduction);

        // Thuế TNCN lũy tiến từng phần
        result.PersonalIncomeTax = _formulaEngine.ComputePersonalIncomeTax(
            result.GrossSalary, 
            totalNonTaxable, 
            result.TotalInsurance, 
            result.PersonalDeduction + result.DependentDeduction);

        // 7. Tổng khấu trừ & Net
        result.TotalDeductions = result.TotalInsurance + result.PersonalIncomeTax;
        result.NetSalary = _formulaEngine.ComputeNetSalary(result.GrossSalary, result.TotalDeductions);

        // 8. Lời bình luận phân tích AI
        var aiSb = new StringBuilder();
        aiSb.Append($"Với mức lương cơ bản **{result.BaseSalary.ToString("N0", VnCulture)} đ**, làm **{result.ActualWorkDays}/{result.StandardWorkDays} ngày công**");
        if (result.OvertimeHours > 0)
        {
            aiSb.Append($" kèm **{result.OvertimeHours} giờ tăng ca (hệ số {request.OvertimeMultiplier})**");
        }
        aiSb.Append($", tổng thu nhập gộp (Gross) của bạn ước đạt **{result.GrossSalary.ToString("N0", VnCulture)} đ**.");
        aiSb.Append($" Sau khi trừ **{result.TotalInsurance.ToString("N0", VnCulture)} đ** tiền bảo hiểm (10.5%)");
        if (result.PersonalIncomeTax > 0)
        {
            aiSb.Append($" và **{result.PersonalIncomeTax.ToString("N0", VnCulture)} đ** thuế TNCN");
        }
        else
        {
            aiSb.Append(" (thu nhập chưa đến ngưỡng nộp thuế TNCN)");
        }
        aiSb.Append($", **tiền lương thực lĩnh chuyển khoản (Net) dự kiến là {result.NetSalary.ToString("N0", VnCulture)} đ**.");

        result.AiAnalysis = aiSb.ToString();
        return result;
    }

    #region Private Helper Methods

    private static string GetLineDiffReason(
        PayslipLineType type, 
        decimal amountA, 
        decimal amountB, 
        Payslip current, 
        Payslip prev)
    {
        decimal diff = amountA - amountB;
        if (diff == 0) return "Không thay đổi";

        return type switch
        {
            PayslipLineType.BASE_SALARY_PRORATED => diff < 0 
                ? $"Giảm {Math.Abs(diff).ToString("N0", VnCulture)} đ do làm {current.ActualWorkDays} ngày (kỳ trước làm {prev.ActualWorkDays} ngày)"
                : $"Tăng {diff.ToString("N0", VnCulture)} đ do số ngày làm việc nhiều hơn",
            PayslipLineType.BONUS => diff < 0 
                ? $"Kỳ trước có thưởng {amountB.ToString("N0", VnCulture)} đ (KPI/tiến độ), kỳ này không có"
                : $"Kỳ này được bổ sung khoản thưởng {amountA.ToString("N0", VnCulture)} đ",
            PayslipLineType.MEAL_ALLOWANCE => "Biến động phụ cấp ăn trưa",
            PayslipLineType.FUEL_ALLOWANCE => "Biến động phụ cấp xăng xe",
            PayslipLineType.OVERTIME_PAY => diff < 0 ? "Giảm tiền làm thêm giờ" : "Tăng tiền làm thêm giờ",
            PayslipLineType.DEDUCTION_BHXH => "Điều chỉnh trích nộp BHXH",
            PayslipLineType.DEDUCTION_BHYT => "Điều chỉnh trích nộp BHYT",
            PayslipLineType.DEDUCTION_BHTN => "Điều chỉnh trích nộp BHTN",
            PayslipLineType.PERSONAL_INCOME_TAX => diff < 0 ? "Thuế TNCN giảm do thu nhập giảm" : "Thuế TNCN tăng",
            _ => diff > 0 ? $"+{diff.ToString("N0", VnCulture)} đ" : $"{diff.ToString("N0", VnCulture)} đ"
        };
    }

    private static void IdentifyKeyFactors(SalaryComparisonDiff diff, Payslip current, Payslip prev)
    {
        decimal daysDiff = current.ActualWorkDays - prev.ActualWorkDays;
        if (daysDiff < 0)
        {
            decimal proratedLineDiff = (current.Lines.FirstOrDefault(l => l.LineType == PayslipLineType.BASE_SALARY_PRORATED)?.Amount ?? 0m)
                                     - (prev.Lines.FirstOrDefault(l => l.LineType == PayslipLineType.BASE_SALARY_PRORATED)?.Amount ?? 0m);

            diff.KeyFactors.Add(new SalaryChangeFactor
            {
                Icon = "fa-calendar-xmark",
                BadgeClass = "bg-danger",
                Title = "Nghỉ không hưởng lương",
                ImpactAmount = proratedLineDiff,
                Description = $"Bạn làm việc {current.ActualWorkDays} ngày công, ít hơn kỳ trước {Math.Abs(daysDiff):0.#} ngày (nghỉ không lương), làm giảm {Math.Abs(proratedLineDiff).ToString("N0", VnCulture)} đ tiền lương công."
            });
        }
        else if (daysDiff > 0)
        {
            diff.KeyFactors.Add(new SalaryChangeFactor
            {
                Icon = "fa-calendar-check",
                BadgeClass = "bg-success",
                Title = "Tăng ngày công làm việc",
                ImpactAmount = 0m,
                Description = $"Bạn đi làm nhiều hơn {daysDiff:0.#} ngày công so với kỳ trước."
            });
        }

        // Kiểm tra tiền thưởng
        decimal bonusA = current.Lines.FirstOrDefault(l => l.LineType == PayslipLineType.BONUS)?.Amount ?? 0m;
        decimal bonusB = prev.Lines.FirstOrDefault(l => l.LineType == PayslipLineType.BONUS)?.Amount ?? 0m;
        if (bonusB > 0 && bonusA == 0)
        {
            diff.KeyFactors.Add(new SalaryChangeFactor
            {
                Icon = "fa-gift",
                BadgeClass = "bg-warning text-dark",
                Title = "Không có khoản thưởng tiến độ/KPI",
                ImpactAmount = -bonusB,
                Description = $"Kỳ trước bạn nhận được {bonusB.ToString("N0", VnCulture)} đ tiền thưởng tiến độ/KPI, kỳ này không có khoản này."
            });
        }
        else if (bonusA > bonusB)
        {
            diff.KeyFactors.Add(new SalaryChangeFactor
            {
                Icon = "fa-gift",
                BadgeClass = "bg-success",
                Title = "Thưởng hiệu suất/KPI bổ sung",
                ImpactAmount = bonusA - bonusB,
                Description = $"Kỳ này bạn được thưởng thêm {(bonusA - bonusB).ToString("N0", VnCulture)} đ."
            });
        }

        // Kiểm tra bảo hiểm
        decimal dedDiff = current.TotalDeductions - prev.TotalDeductions;
        if (Math.Abs(dedDiff) > 1000m)
        {
            diff.KeyFactors.Add(new SalaryChangeFactor
            {
                Icon = "fa-shield-halved",
                BadgeClass = "bg-info text-dark",
                Title = "Biến động các khoản trích nộp bảo hiểm/thuế",
                ImpactAmount = -dedDiff,
                Description = $"Các khoản trích nộp thay đổi {dedDiff.ToString("+#,##0;-#,##0;0", VnCulture)} đ."
            });
        }
    }

    private static string GenerateAiSummary(
        SalaryComparisonDiff diff, 
        Payslip current, 
        Payslip prev, 
        Employee? employee)
    {
        string name = employee?.FullName ?? current.EmployeeName ?? "anh/chị";
        string periodCurrName = current.PeriodMonth > 0 ? $"Tháng {current.PeriodMonth}/{current.PeriodYear}" : (current.PeriodCode ?? "Kỳ hiện tại");
        string periodPrevName = prev.PeriodMonth > 0 ? $"Tháng {prev.PeriodMonth}/{prev.PeriodYear}" : (prev.PeriodCode ?? "Kỳ trước");

        var sb = new StringBuilder();
        sb.AppendLine($"Chào **{name}**, em đã đối soát phiếu lương **{periodCurrName}** so với **{periodPrevName}** của bạn:");
        sb.AppendLine();

        foreach (var factor in diff.KeyFactors)
        {
            string sign = factor.ImpactAmount > 0 ? "+" : (factor.ImpactAmount < 0 ? "-" : "");
            string impactStr = factor.ImpactAmount != 0 ? $": **{sign}{Math.Abs(factor.ImpactAmount).ToString("N0", VnCulture)} đ**" : "";
            sb.AppendLine($"• **{factor.Title}**{impactStr} — {factor.Description}");
        }

        decimal netDiff = diff.NetDiff;
        string netSign = netDiff > 0 ? "tăng" : (netDiff < 0 ? "giảm" : "không đổi");
        string netDiffAbs = Math.Abs(netDiff).ToString("N0", VnCulture);

        sb.AppendLine();
        if (netDiff != 0)
        {
            sb.AppendLine($"👉 **Tổng kết:** Thực lĩnh kỳ này là **{current.NetSalary.ToString("N0", VnCulture)} đ**, {netSign} **{netDiffAbs} đ** so với kỳ trước ({prev.NetSalary.ToString("N0", VnCulture)} đ).");
        }
        else
        {
            sb.AppendLine($"👉 **Tổng kết:** Thực lĩnh cả 2 kỳ đều đạt **{current.NetSalary.ToString("N0", VnCulture)} đ** (không thay đổi).");
        }

        return sb.ToString();
    }

    #endregion
}
