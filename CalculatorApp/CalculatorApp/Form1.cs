using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CalculatorApp
{
    public partial class Form1 : Form
    {
        /// <summary>最初の値</summary>
        private decimal FirstValue = 0m;

        /// <summary>2番目の値</summary>
        private decimal SecondValue = 0m;

        /// <summary>結果欄の上書き入力フラグ</summary>
        private bool TextOverwrite = false;

        /// <summary>小数点入力済みフラグ</summary>
        private bool NumDot = false;

        /// <summary>エラー状態</summary>
        private bool IsErrorState = false;

        /// <summary>現在の入力を CE でクリアしたか</summary>
        private bool IsClearEntry = false;

        /// <summary>内部の現在表示値（表示文字列と分離）</summary>
        private decimal DisplayValue = Constants.Numeric.INITIAL_VALUE;

        /// <summary>±押下時にフォーマット保持するか</summary>
        private bool PreserveFormatOnToggle = false;

        /// <summary>直近のユーザー生入力（カンマなし＝編集バッファの唯一のソース）</summary>
        private string lastUserTypedRaw = Constants.Numeric.ZERO_VALUE;

        /// <summary>直前が％か</summary>
        private bool lastActionWasPercent = false;

        /// <summary>＝直後に途中式を消したか</summary>
        private bool ClearedExprAfterEqual = false;

        /// <summary>基準フォントサイズ（初期値）</summary>
        private float defaultFontSize;

        /// <summary>途中式欄の基準フォントサイズ（初期値）</summary>
        private float defaultExpressionFontSize;

        /// <summary>±直近押下</summary>
        private bool isNegated = false;

        /// <summary>エラー時に無効化するボタン（＝は含めない）</summary>
        private Button[] DisabledButtonsOnError;

        /// <summary>% などで自動生成された右辺を編集不可にする</summary>
        private bool lockRhsAfterAutoOp = false;


        private enum OperatorType
        {
            NON, ADD, SUBTRACT, MULTIPLY, DIVIDE, PERCENT
        }

        private OperatorType currentOperatorType = OperatorType.NON;

        public Form1()
        {
            InitializeComponent();

            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            DisabledButtonsOnError = new Button[]
            {
                btnDot, btnTogglesign, btnPercent, btnPlus,
                btnMinus, btnMultiply, btnDivide, btnEnter
            };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textResult.Text = Constants.Numeric.ZERO_VALUE;
            TextOverwrite = true;

            textResult.Font = new Font(textResult.Font.FontFamily, Constants.FontSize.RESULT_DISPLAY_BASE, textResult.Font.Style);
            textExpression.Font = new Font(textExpression.Font.FontFamily, Constants.FontSize.EXPRESSION_DISPLAY_BASE, textExpression.Font.Style);

            defaultFontSize = textResult.Font.Size;
            defaultExpressionFontSize = textExpression.Font.Size;

            textResult.ReadOnly = true;
            textResult.TextAlign = HorizontalAlignment.Right;
            textResult.BorderStyle = BorderStyle.None;

            textExpression.ReadOnly = true;
            textExpression.TextAlign = HorizontalAlignment.Right;
            textExpression.BorderStyle = BorderStyle.None;

            // 内部値の初期同期
            DisplayValue = 0m;
            lastUserTypedRaw = "0";
        }

        private void textResult_TextChanged(object sender, EventArgs e)
        {
            AutoFitResultFont();
        }

        private void textExpression_TextChanged(object sender, EventArgs e)
        {
        }

        private void btnNum_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            if (IsError()) { ResetCalculatorState(); }
            OnDigitButton(btn.Text);
        }

        private void btnDot_Click(object sender, EventArgs e)
        {
            HandleInitialState();
            OnDotButton();
        }

        private void btnOperation_Click(object sender, EventArgs e)
        {
            var btn = sender as Button; if (btn == null) return;
            var op = OperatorType.NON;
            switch (btn.Text)
            {
                case Constants.Symbol.ADD: op = OperatorType.ADD; break;
                case Constants.Symbol.SUBTRACT: op = OperatorType.SUBTRACT; break;
                case Constants.Symbol.MULTIPLY: op = OperatorType.MULTIPLY; break;
                case Constants.Symbol.DIVIDE: op = OperatorType.DIVIDE; break;
            }
            OnOperatorButton(op);
        }

        private void btnEnter_Click(object sender, EventArgs e)
        {
            OnEqualsButton();
        }

        private void btnPercent_Click(object sender, EventArgs e)
        {
            OnPercentButton();
        }

        private void btnClearEntry_Click(object sender, EventArgs e)
        {
            OnClearEntryButton();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            OnClearButton();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            OnBackspaceButton();
        }

        private void btnTogglesign_Click(object sender, EventArgs e)
        {
            OnToggleSignButton();
        }

        private void btnTopMost_Click(object sender, EventArgs e)
        {
            this.TopMost = !this.TopMost;
        }

        // ===== 入力処理：編集は raw に対してのみ。内部値は raw を元に更新 =====

        private void OnDigitButton(string digit)
        {
            HandleInitialState();
            SetButtonsEnabled(true);
            lastActionWasPercent = false;

            if (IsExponentDisplay())
            {
                // 指数表示からの直接追記は上書き開始に切替
                TextOverwrite = true;
                NumDot = false;
                lastUserTypedRaw = "0";
            }

            string currentRaw = lastUserTypedRaw;
            if (!IsInputValid(currentRaw, digit)) return;

            if (TextOverwrite)
            {
                StartNewNumber(digit);
            }
            else
            {
                AppendDigit(digit);
            }

            // raw -> 内部
            if (decimal.TryParse(lastUserTypedRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var dv))
            {
                DisplayValue = dv;
            }

            // 画面
            textResult.Text = InsertCommasIfNeeded(lastUserTypedRaw, NumDot);
            IsClearEntry = false;
            PreserveFormatOnToggle = true;
        }

        private void OnDotButton()
        {
            HandleInitialState();
            lastActionWasPercent = false;

            if (NumDot) return;

            if (TextOverwrite)
            {
                lastUserTypedRaw = "0.";
                textResult.Text = lastUserTypedRaw;
                TextOverwrite = false;
            }
            else
            {
                lastUserTypedRaw += ".";
                textResult.Text = lastUserTypedRaw;
            }
            NumDot = true;

            // raw -> 内部（"0." は一時的に 0 として保持）
            if (decimal.TryParse(lastUserTypedRaw == "0." ? "0" : lastUserTypedRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var dv))
            {
                DisplayValue = dv;
            }

            PreserveFormatOnToggle = true;
        }

        private void OnOperatorButton(OperatorType op)
        {
            if (IsError()) { ResetCalculatorState(); return; }

            try
            {
                // ===== 1) 末尾が演算子なら「置換」して終了（negate(...) 表示中でも同様） =====
                string curExpr = (textExpression.Text ?? "").Trim();
                if (!string.IsNullOrEmpty(curExpr) && !curExpr.EndsWith(Constants.Symbol.EQUAL))
                {
                    string[] ops = { Constants.Symbol.ADD, Constants.Symbol.SUBTRACT, Constants.Symbol.MULTIPLY, Constants.Symbol.DIVIDE };
                    foreach (var o in ops)
                    {
                        if (curExpr.EndsWith(o))
                        {
                            textExpression.Text = curExpr.Substring(0, curExpr.Length - o.Length) + GetOperatorSymbol(op);
                            currentOperatorType = op;

                            // 右辺編集ロックは解除（次の入力を受け付ける）
                            lockRhsAfterAutoOp = false;
                            lastActionWasPercent = false;
                            TextOverwrite = true;
                            NumDot = false;
                            return;
                        }
                    }
                }

                // ===== 2) = の直後：現在値を左辺として新規開始 =====
                if (ExpressionEndsWithEqual())
                {
                    FirstValue = GetCurrentValue();
                    SecondValue = Constants.Numeric.INITIAL_VALUE;
                    currentOperatorType = op;

                    UpdateExpressionDisplay(FirstValue, currentOperatorType);
                    DisplayNumber(FirstValue, true);

                    // フラグ類
                    TextOverwrite = true;
                    NumDot = false;
                    lastActionWasPercent = false;
                    PreserveFormatOnToggle = false;
                    lockRhsAfterAutoOp = false;
                    return;
                }

                // ===== 3) % の直後に演算子：右辺を確定して連鎖計算 =====
                if (lastActionWasPercent && currentOperatorType != OperatorType.NON)
                {
                    var cur = GetCurrentValue();          // 右辺（%で置換済み）
                    PerformPendingCalculation(cur);       // A op cur
                    if (IsError()) return;

                    DisplayNumber(FirstValue, true);
                    currentOperatorType = op;
                    UpdateExpressionDisplay(FirstValue, currentOperatorType);

                    // フラグ類
                    TextOverwrite = true;
                    NumDot = false;
                    lastActionWasPercent = false;
                    PreserveFormatOnToggle = false;
                    lockRhsAfterAutoOp = false;
                    return;
                }

                // ===== 4) CE で右辺クリア直後など：演算子だけ差し替え =====
                if (IsClearEntry)
                {
                    currentOperatorType = op;
                    UpdateExpressionDisplay(FirstValue, currentOperatorType);
                    DisplayNumber(FirstValue, true);

                    IsClearEntry = false;
                    TextOverwrite = true;
                    NumDot = false;
                    lastActionWasPercent = false;
                    PreserveFormatOnToggle = false;
                    lockRhsAfterAutoOp = false;
                    return;
                }

                // ===== 5) 右辺未入力で演算子だけ変えたいケース（A op のまま） =====
                if (TextOverwrite && currentOperatorType != OperatorType.NON)
                {
                    currentOperatorType = op;
                    UpdateExpressionDisplay(FirstValue, currentOperatorType);

                    TextOverwrite = true;
                    NumDot = false;
                    lastActionWasPercent = false;
                    PreserveFormatOnToggle = false;
                    lockRhsAfterAutoOp = false;
                    return;
                }

                // ===== 6) 通常ケース：A op B を計算 → op 更新 =====
                var currentValue = GetCurrentValue();     // 右辺 B
                PerformPendingCalculation(currentValue);  // A op B
                if (IsError()) return;

                DisplayNumber(FirstValue, true);
                currentOperatorType = op;
                UpdateExpressionDisplay(FirstValue, currentOperatorType);

                // フラグ類
                TextOverwrite = true;
                NumDot = false;
                lastActionWasPercent = false;
                PreserveFormatOnToggle = false;
                lockRhsAfterAutoOp = false;
            }
            catch (OverflowException)
            {
                SetErrorState(Constants.ErrorMessage.OVERFLOW);
            }
        }


        private void OnEqualsButton()
        {
            if (ShouldResetOnError()) return;

            try
            {
                var result = ProcessEqualsLogic();
                if (IsError()) return;
                DisplayNumber(result, true);

                PreserveFormatOnToggle = false;
                lastActionWasPercent = false;
            }
            catch (InvalidOperationException ex)
            {
                SetErrorState(ex.Message);
            }
            catch (OverflowException)
            {
                SetErrorState(Constants.ErrorMessage.OVERFLOW);
            }
        }

        private void OnPercentButton()
        {
            if (ShouldResetOnError()) return;

            // ＝直後 or 単独値 → 常に「現在値 ÷ 100」
            if (ExpressionEndsWithEqual() || currentOperatorType == OperatorType.NON)
            {
                var r = GetCurrentValue();            // いま画面にある数
                var v = CalculatePercent(r);          // r / 100

                FirstValue = v;                       // 単独値として確定
                SecondValue = Constants.Numeric.INITIAL_VALUE;
                currentOperatorType = OperatorType.NON;

                DisplayNumber(v, true);               // 上書き開始状態で表示
                textExpression.Text = FormatNumberForExpression(v);

                lastActionWasPercent = true;
                PreserveFormatOnToggle = false;
                return;
            }

            // 途中の二項演算中（A op B）で％
            // 「B 未入力（= TextOverwrite）なら B は 0 とみなす」のではなく、
            // Windows 電卓と同じく、押した瞬間の「現在値」を使う
            var rhs = GetCurrentValue();              // いま表示中の右辺（未入力なら 0 ではなく表示値）
            var percent = CalculatePercent(rhs);      // rhs / 100

            decimal replacedB;
            if (currentOperatorType == OperatorType.ADD || currentOperatorType == OperatorType.SUBTRACT)
            {
                replacedB = FirstValue * percent;     // + / - は A × (B/100)
            }
            else
            {
                replacedB = percent;                  // × / ÷ は B/100
            }

            // 右辺を置き換えた状態を画面に反映（＝は付けない）
            textExpression.Text = string.Format("{0} {1} {2}",
                FormatNumberForExpression(FirstValue),
                GetOperatorSymbol(currentOperatorType),
                FormatNumberForExpression(replacedB));

            DisplayNumber(replacedB, false);          // 右辺（B）を結果欄に表示し、編集継続可能に

            lastActionWasPercent = true;
            PreserveFormatOnToggle = false;
        }


        private void OnClearEntryButton()
        {
            if (ShouldResetOnError()) return;

            var currentExpression = textExpression.Text != null ? textExpression.Text.Trim() : string.Empty;

            if (ExpressionEndsWithEqual())
            {
                if (HasBinaryOperatorInExpression(currentExpression))
                {
                    ResetCalculatorState();
                    SetButtonsEnabled(true);
                    return;
                }

                DisplayZeroResult();
                ResetCalculationValues();
                return;
            }

            ClearCurrentEntry();
        }

        private bool HasBinaryOperatorInExpression(string expr)
        {
            if (string.IsNullOrEmpty(expr)) return false;

            int eq = expr.LastIndexOf(Constants.Symbol.EQUAL);
            string body = (eq >= 0) ? expr.Substring(0, eq) : expr;

            return body.Contains(Constants.Symbol.ADD) ||
                   body.Contains(Constants.Symbol.SUBTRACT) ||
                   body.Contains(Constants.Symbol.MULTIPLY) ||
                   body.Contains(Constants.Symbol.DIVIDE);
        }

        private void DisplayZeroResult()
        {
            textResult.Text = Constants.Numeric.ZERO_VALUE;
            TextOverwrite = true;
            NumDot = false;

            // 内部もゼロに同期
            DisplayValue = 0m;
            lastUserTypedRaw = "0";

            PreserveFormatOnToggle = false;
            lastActionWasPercent = false;
        }

        private void ResetCalculationValues()
        {
            FirstValue = Constants.Numeric.INITIAL_VALUE;
            SecondValue = Constants.Numeric.INITIAL_VALUE;
            currentOperatorType = OperatorType.NON;
        }

        private void ClearCurrentEntry()
        {
            IsClearEntry = true;
            DisplayZeroResult();
        }

        private void OnClearButton()
        {
            ResetAllState();
        }

        private void OnBackspaceButton()
        {
            if (ShouldResetOnError()) return;

            if (ExpressionEndsWithEqual())
            {
                textExpression.Text = "";
                TextOverwrite = true;
                NumDot = false;
                PreserveFormatOnToggle = false;
                lastActionWasPercent = false;
                ClearedExprAfterEqual = true;

                // 編集バッファと内部の初期化
                lastUserTypedRaw = "0";
                DisplayValue = 0m;
                return;
            }

            if (ClearedExprAfterEqual) return;

            if (IsExponentDisplay())
            {
                // 指数表示からのBackspaceは新規入力開始
                TextOverwrite = true;
                NumDot = false;
                textResult.Text = Constants.Numeric.ZERO_VALUE;

                lastUserTypedRaw = "0";
                DisplayValue = 0m;

                PreserveFormatOnToggle = false;
                lastActionWasPercent = false;
                return;
            }

            if (TextOverwrite) return;

            // raw を1文字削る
            if (lastUserTypedRaw.Length > 0)
            {
                string newRaw = lastUserTypedRaw.Substring(0, lastUserTypedRaw.Length - 1);
                if (string.IsNullOrEmpty(newRaw) || newRaw == "-")
                {
                    lastUserTypedRaw = "0";
                    TextOverwrite = true;
                    NumDot = false;
                }
                else
                {
                    lastUserTypedRaw = newRaw;
                    NumDot = lastUserTypedRaw.Contains(".");
                }
            }
            else
            {
                ResetCalculatorState();
                return;
            }

            // raw -> 内部
            if (decimal.TryParse(lastUserTypedRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var dv))
            {
                DisplayValue = dv;
            }

            // 画面
            textResult.Text = InsertCommasIfNeeded(lastUserTypedRaw, NumDot);

            PreserveFormatOnToggle = true;
            lastActionWasPercent = false;
        }

        private void OnToggleSignButton()
        {
            if (ShouldResetOnError()) return;
            if (string.IsNullOrEmpty(textResult.Text)) return;

            if (PreserveFormatOnToggle && !IsExponentDisplay())
            {
                string raw = lastUserTypedRaw;
                raw = ToggleSignRaw(raw);
                lastUserTypedRaw = raw;

                // raw -> 内部
                if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var dv))
                    DisplayValue = dv;

                // 画面
                textResult.Text = InsertCommasIfNeeded(raw, NumDot);

                TextOverwrite = false;
                NumDot = (raw.IndexOf('.') >= 0);

                UpdateExpressionForToggleSign();

                lastActionWasPercent = false;
                return;
            }

            // 指数や確定後は内部値だけ符号反転し、表示は再整形
            DisplayValue = -GetCurrentValue();
            DisplayNumber(DisplayValue, false);
            UpdateExpressionForToggleSign();

            PreserveFormatOnToggle = false;
            lastActionWasPercent = false;

            // 編集バッファも同期（次の編集を自然に）
            lastUserTypedRaw = DisplayValue.ToString("0.#############################", CultureInfo.InvariantCulture);
            NumDot = lastUserTypedRaw.Contains(".");
        }

        private void StartNewNumber(string digit)
        {
            lastUserTypedRaw = digit;
            textResult.Text = digit;
            TextOverwrite = false;
            NumDot = (digit == ".");

            if (decimal.TryParse(lastUserTypedRaw == "." ? "0" : lastUserTypedRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var dv))
                DisplayValue = dv;
        }

        private void AppendDigit(string digit)
        {
            lastUserTypedRaw += digit;
            textResult.Text = InsertCommasIfNeeded(lastUserTypedRaw, NumDot);

            if (decimal.TryParse(lastUserTypedRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var dv))
                DisplayValue = dv;
        }

        private bool IsInputValid(string currentRaw, string digit)
        {
            bool startsWithZeroDot = currentRaw.StartsWith("0.") || currentRaw.StartsWith("-0.");
            int maxDigits = startsWithZeroDot ? Constants.Numeric.MAX_FRACTION_DISPLAY_DIGITS_LEADING_ZERO : Constants.Numeric.MAX_INTEGER_DISPLAY_DIGITS;

            string nextText = TextOverwrite ? digit : currentRaw + digit;
            int nextLength = nextText.Replace(".", "").Replace("-", "").Length;

            if (nextLength > maxDigits) return false;

            if (!TextOverwrite && currentRaw == Constants.Numeric.ZERO_VALUE && digit == Constants.Numeric.ZERO_VALUE && !NumDot) return false;

            return true;
        }

        private string GetOperatorSymbol(OperatorType type)
        {
            switch (type)
            {
                case OperatorType.ADD: return Constants.Symbol.ADD;
                case OperatorType.SUBTRACT: return Constants.Symbol.SUBTRACT;
                case OperatorType.MULTIPLY: return Constants.Symbol.MULTIPLY;
                case OperatorType.DIVIDE: return Constants.Symbol.DIVIDE;
                default: return string.Empty;
            }
        }

        private decimal Calculate(decimal left, decimal right, OperatorType type)
        {
            switch (type)
            {
                case OperatorType.ADD: return left + right;
                case OperatorType.SUBTRACT: return left - right;
                case OperatorType.MULTIPLY: return left * right;
                case OperatorType.DIVIDE: return left / right;
                default: return right;
            }
        }

        private void PerformPendingCalculation(decimal currentValue)
        {
            if (ExpressionEndsWithEqual() || currentOperatorType == OperatorType.NON)
            {
                FirstValue = currentValue;
            }
            else
            {
                if (currentOperatorType == OperatorType.DIVIDE && currentValue == Constants.Numeric.INITIAL_VALUE)
                {
                    if (FirstValue == Constants.Numeric.INITIAL_VALUE)
                        SetErrorState(Constants.ErrorMessage.UNDEFINED);
                    else
                        SetErrorState(Constants.ErrorMessage.DIVIDE_BY_ZERO);
                    return;
                }

                decimal result = Calculate(FirstValue, currentValue, currentOperatorType);
                FirstValue = result;
            }
        }

        private void UpdateExpressionDisplay(decimal value, OperatorType type)
        {
            string op = GetOperatorSymbol(type);
            string currentExpr = (textExpression.Text != null ? textExpression.Text.Trim() : string.Empty);

            if (!string.IsNullOrEmpty(currentExpr) &&
                !currentExpr.EndsWith(Constants.Symbol.EQUAL) &&
                currentExpr.StartsWith(Constants.SpecialDisplay.NEGATE_FUNCTION + "(", StringComparison.Ordinal))
            {
                textExpression.Text = currentExpr + " " + op;
                return;
            }

            // ★ 修正点：途中式欄も結果欄と同じ丸め・整形を通す
            textExpression.Text = string.Format("{0} {1}", FormatNumberForExpression(value), op);
        }

        private decimal ProcessEqualsLogic()
        {
            decimal currentValue = GetCurrentValue();
            bool isFirstEqual = !ExpressionEndsWithEqual();

            if (currentOperatorType == OperatorType.NON)
            {
                SecondValue = currentValue;
                textExpression.Text = string.Format("{0} {1}",
                    FormatNumberForExpression(currentValue), Constants.Symbol.EQUAL);
                FirstValue = currentValue;
                return currentValue;
            }

            decimal left, right;

            if (isFirstEqual)
            {
                left = FirstValue;
                right = currentValue;
                SecondValue = currentValue;
            }
            else
            {
                left = FirstValue;
                right = SecondValue;
            }

            if (currentOperatorType == OperatorType.DIVIDE && right == Constants.Numeric.INITIAL_VALUE)
            {
                if (left == Constants.Numeric.INITIAL_VALUE)
                    throw new InvalidOperationException(Constants.ErrorMessage.UNDEFINED);
                else
                    throw new InvalidOperationException(Constants.ErrorMessage.DIVIDE_BY_ZERO);
            }

            decimal result = Calculate(left, right, currentOperatorType);
            FirstValue = result;

            string opSym = GetOperatorSymbol(currentOperatorType);
            string leftExpr = FormatNumberForExpression(left);   // ★ 丸め後に表示
            string rightExpr = FormatNumberForExpression(right); // ★ 丸め後に表示

            string curr = (textExpression.Text != null ? textExpression.Text.Trim() : string.Empty);

            if (!string.IsNullOrEmpty(curr) &&
                !curr.EndsWith(Constants.Symbol.EQUAL) &&
                curr.StartsWith(Constants.SpecialDisplay.NEGATE_FUNCTION + "(", StringComparison.Ordinal))
            {
                if (curr.EndsWith(opSym))
                    textExpression.Text = curr + " " + rightExpr + " " + Constants.Symbol.EQUAL;
                else
                    textExpression.Text = curr + " " + Constants.Symbol.EQUAL;
            }
            else
            {
                textExpression.Text = string.Format("{0} {1} {2} {3}",
                    leftExpr, opSym, rightExpr, Constants.Symbol.EQUAL);
            }

            return result;
        }

        private decimal CalculatePercent(decimal value) => value * Constants.Numeric.PERCENT_MULTIPLY;

        private void UpdatePercentDisplay(decimal percentValue)
        {
            decimal previousValue = FirstValue;
            decimal calculatedValue;

            if (currentOperatorType == OperatorType.ADD || currentOperatorType == OperatorType.SUBTRACT)
            {
                calculatedValue = previousValue * percentValue;
            }
            else
            {
                calculatedValue = percentValue; // B% = B/100
            }

            textExpression.Text = string.Format("{0} {1} {2}",
                FormatNumberForExpression(previousValue),
                GetOperatorSymbol(currentOperatorType),
                FormatNumberForExpression(calculatedValue));

            DisplayNumber(calculatedValue, false);
        }

        private string ToggleSignRaw(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return raw;
            if (raw[0] == '-') return raw.Substring(1);
            return "-" + raw;
        }

        private void UpdateExpressionForToggleSign()
        {
            string expr = (textExpression.Text != null ? textExpression.Text.Trim() : string.Empty);

            if (expr.EndsWith(Constants.Symbol.EQUAL))
            {
                int eq = expr.LastIndexOf(Constants.Symbol.EQUAL);
                string body = (eq >= 0 ? expr.Substring(0, eq) : expr).Trim();
                if (string.IsNullOrEmpty(body)) body = FormatNumberForExpression(FirstValue);

                textExpression.Text = Constants.SpecialDisplay.NEGATE_FUNCTION + "(" + body + ")";
                return;
            }

            if (expr.StartsWith(Constants.SpecialDisplay.NEGATE_FUNCTION + "(", StringComparison.Ordinal))
            {
                textExpression.Text = Constants.SpecialDisplay.NEGATE_FUNCTION + "(" + expr + ")";
            }
        }

        // ===== 重要：表示はここだけ。内部値はここ以外で丸めない =====

        private void DisplayNumber(decimal value, bool overwrite)
        {
            string s;

            decimal abs = Math.Abs(value);
            if (abs != 0m && (abs < Constants.Numeric.SCI_SMALL_THRESHOLD || abs >= Constants.Numeric.SCI_LARGE_THRESHOLD))
            {
                // 指数レンジは事前丸めなしで指数整形（有効桁17）
                s = FormatExponential(value);
            }
            else
            {
                var rounded = RoundResult(value); // 固定小数のときだけ丸め
                s = FormatNumberForDisplay(rounded);
            }

            textResult.Text = s;
            if (!IsExponentDisplay()) UpdateTextResultWithCommas();

            TextOverwrite = overwrite;
            NumDot = false;

            // 内部（DisplayValue）にも反映（※指数文字列は一切パースしない）
            DisplayValue = value;

            PreserveFormatOnToggle = false;
            lastUserTypedRaw = IsExponentDisplay()
                ? DisplayValue.ToString("0.#############################", CultureInfo.InvariantCulture)
                : textResult.Text.Replace(",", "");
        }

        private string InsertCommasIfNeeded(string raw, bool numDot)
        {
            // 生の編集テキストに 3桁区切りを付ける（簡易）
            if (string.IsNullOrEmpty(raw) || raw == "-" || (raw == "0" && !numDot)) return raw;

            bool neg = raw.StartsWith("-");
            if (neg) raw = raw.Substring(1);

            int dot = raw.IndexOf('.');
            string intPart = dot >= 0 ? raw.Substring(0, dot) : raw;
            string fracPart = dot >= 0 ? raw.Substring(dot + 1) : "";

            if (decimal.TryParse(intPart, NumberStyles.Number, CultureInfo.InvariantCulture, out var iv))
            {
                string intFmt = iv.ToString("#,##0", CultureInfo.InvariantCulture);
                string newText = (dot >= 0) ? (intFmt + "." + fracPart) : intFmt;
                if (neg) newText = "-" + newText;
                return newText;
            }
            return raw;
        }

        private void UpdateTextResultWithCommas()
        {
            if (IsError()) return;
            if (IsExponentDisplay()) return;

            // 既にカンマ入りの可能性があるので一旦素に戻す
            string raw = textResult.Text.Replace(",", "");
            bool hasDot = raw.Contains(".");
            string formatted = InsertCommasIfNeeded(raw, hasDot);

            if (formatted != textResult.Text)
            {
                // キャレット位置を極力維持
                int fromEnd = textResult.Text.Length - textResult.SelectionStart;
                textResult.Text = formatted;
                textResult.SelectionStart = Math.Max(0, textResult.Text.Length - fromEnd);
            }
        }

        // ★ 修正点：途中式欄の数値も結果欄と同じ丸めポリシーで描画
        private string FormatNumberForExpression(decimal value)
        {
            var rounded = RoundResult(value);
            return FormatNumberForDisplay(rounded);
        }

        private static decimal PowerOf10(int exponent)
        {
            decimal result = 1m;
            for (int i = 0; i < exponent; i++) result *= 10m;
            return result;
        }

        private string FormatExponential(decimal value)
        {
            // Windows 電卓寄せ：有効桁 17 桁
            const int SIG = Constants.Numeric.MAX_SIGNIFICANT_DIGITS; // 17

            // まず最大精度で指数化（既定の "e" は小数点以下6桁なので、これを回避する）
            string expFull = value.ToString("e28", CultureInfo.InvariantCulture);
            string[] parts = expFull.Split('e');
            // parts[0] = 仮数文字列、parts[1] = 指数部分（例 "+17", "-10"）

            if (!decimal.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out decimal mant))
            {
                // 万一 parsing に失敗したら元文字列を返す
                return expFull;
            }

            int exp = int.Parse(parts[1], CultureInfo.InvariantCulture);

            int sign = mant < 0 ? -1 : 1;
            mant = Math.Abs(mant);

            // 有効桁 SIG に丸め
            decimal scale = PowerOf10(SIG - 1);
            mant = Math.Round(mant * scale, 0, MidpointRounding.AwayFromZero) / scale;

            // 仮数が 10.000... になるなら 1 桁繰り上げ
            if (mant >= 10m)
            {
                mant /= 10m;
                exp += 1;
            }

            mant *= sign;

            // 仮数文字列化。整数化した場合は末尾ドットを付ける
            string mantStr;
            decimal truncated = decimal.Truncate(mant);
            if (mant == truncated)
            {
                // 整数なので末尾にドット
                mantStr = truncated.ToString("0", CultureInfo.InvariantCulture) + ".";
            }
            else
            {
                // 小数部あり → 小数点以下を丸めてゼロ落とし
                mantStr = mant.ToString("0.#############################", CultureInfo.InvariantCulture).TrimEnd('0');
            }

            // 指数部分を整形（先頭ゼロ除去）
            string expStr = (exp >= 0 ? "+" : "") + exp.ToString(CultureInfo.InvariantCulture);

            return mantStr + "e" + expStr;
        }

        private string FormatNumberForDisplay(decimal value)
        {
            decimal abs = Math.Abs(value);
            if (abs == 0m) return Constants.Numeric.ZERO_VALUE;

            if (abs < Constants.Numeric.SCI_SMALL_THRESHOLD || abs >= Constants.Numeric.SCI_LARGE_THRESHOLD)
                return FormatExponential(value);

            string fixedStr = value.ToString("0.#############################", CultureInfo.InvariantCulture);

            if (abs >= 1m)
            {
                int dot = fixedStr.IndexOf('.');
                bool neg = (fixedStr[0] == '-');
                int intLen = (dot >= 0 ? dot : fixedStr.Length) - (neg ? 1 : 0);
                if (intLen > Constants.Numeric.MAX_INTEGER_DISPLAY_DIGITS)
                    return FormatExponential(value);
                return fixedStr;
            }
            else
            {
                int dot = fixedStr.IndexOf('.');
                int leadingZeros = 0;
                for (int i = dot + 1; i < fixedStr.Length && fixedStr[i] == '0'; i++) leadingZeros++;

                int totalFractionDigits = fixedStr.Length - dot - 1;
                int significantDigits = 0;
                for (int i = dot + 1 + leadingZeros; i < fixedStr.Length; i++)
                    if (char.IsDigit(fixedStr[i])) significantDigits++;

                // ★ MAX_TOTAL_FRACTION_DIGITS = 16 に引き上げ（0.3333…×16桁を許容）
                if (leadingZeros >= Constants.Numeric.MAX_FRACTION_DISPLAY_DIGITS_LEADING_ZERO
                    || totalFractionDigits > Constants.Numeric.MAX_TOTAL_FRACTION_DIGITS
                    || significantDigits > Constants.Numeric.MAX_SIGNIFICANT_DIGITS)
                {
                    return FormatExponential(value);
                }

                return fixedStr;
            }
        }

        // ====== ここが最大のポイント：表示テキストをパースせず、内部の DisplayValue を返す ======
        private decimal GetCurrentValue() => DisplayValue;

        private decimal RoundResult(decimal value)
        {
            decimal abs = Math.Abs(value);
            if (abs == 0m) return 0m;

            // 指数レンジは事前丸め禁止（表示でのみ丸め/指数化）
            if (abs < Constants.Numeric.SCI_SMALL_THRESHOLD || abs >= Constants.Numeric.SCI_LARGE_THRESHOLD)
                return value;

            if (abs > 0m && abs < 1m)
                return Math.Round(value, 16, MidpointRounding.AwayFromZero);

            if (abs >= 1m)
            {
                string integerPartStr = Math.Floor(abs).ToString(CultureInfo.InvariantCulture);
                int integerLength = integerPartStr.Length;
                int decimalPlacesToRound = 16 - integerLength;
                if (decimalPlacesToRound >= 0)
                    return Math.Round(value, decimalPlacesToRound, MidpointRounding.AwayFromZero);
            }
            return value;
        }

        private bool IsError() => IsErrorState;

        private bool ExpressionEndsWithEqual()
            => textExpression.Text.Length > 0 && textExpression.Text.EndsWith(Constants.Symbol.EQUAL);

        private bool IsExponentDisplay()
        {
            string t = textResult.Text;
            return (t.IndexOf('e') >= 0 || t.IndexOf('E') >= 0);
        }

        private bool ShouldResetOnError()
        {
            if (IsErrorState)
            {
                ResetCalculatorState();
                return true;
            }
            return false;
        }

        private void AutoFitResultFont()
        {
            float size = defaultFontSize;
            FontFamily family = textResult.Font.FontFamily;
            FontStyle style = textResult.Font.Style;

            while (size > Constants.FontSize.MIN_LIMIT)
            {
                using (Font trial = new Font(family, size, style))
                {
                    Size proposed = new Size(int.MaxValue, int.MaxValue);
                    TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;
                    Size sz = TextRenderer.MeasureText(textResult.Text, trial, proposed, flags);

                    if (sz.Width <= textResult.ClientSize.Width)
                    {
                        if (Math.Abs(textResult.Font.Size - size) > 0.1f)
                        {
                            Font old = textResult.Font;
                            textResult.Font = new Font(family, size, style);
                            old.Dispose();
                        }
                        return;
                    }
                }
                size -= Constants.FontSize.SIZE_EPSILON;
            }

            if (Math.Abs(textResult.Font.Size - Constants.FontSize.MIN_LIMIT) > Constants.FontSize.REDUCTION_STEP)
            {
                Font oldFinal = textResult.Font;
                textResult.Font = new Font(family, Constants.FontSize.MIN_LIMIT, style);
                oldFinal.Dispose();
            }
        }

        private void SetButtonsEnabled(bool enabled)
        {
            foreach (Button btn in DisabledButtonsOnError) btn.Enabled = enabled;
        }

        private void SetErrorState(string message)
        {
            textResult.Text = message;

            float sz = Constants.FontSize.ERROR_MESSAGE;
            if (sz < Constants.FontSize.MIN_LIMIT) sz = Constants.FontSize.MIN_LIMIT;
            textResult.Font = new Font(textResult.Font.FontFamily, sz, textResult.Font.Style);

            IsErrorState = true;
            SetButtonsEnabled(false);
        }

        private void HandleInitialState()
        {
            if (IsErrorState || ExpressionEndsWithEqual())
            {
                ResetCalculatorState();
                ClearedExprAfterEqual = false;
            }
        }

        private void ResetAllState()
        {
            ResetCalculatorState();
            SetButtonsEnabled(true);
        }

        private void ResetCalculatorState()
        {
            FirstValue = Constants.Numeric.INITIAL_VALUE;
            SecondValue = Constants.Numeric.INITIAL_VALUE;
            currentOperatorType = OperatorType.NON;
            textExpression.Text = "";
            textResult.Text = Constants.Numeric.ZERO_VALUE;
            TextOverwrite = true;
            NumDot = false;
            IsErrorState = false;
            IsClearEntry = false;

            // 内部・編集系の完全初期化
            DisplayValue = 0m;
            lastUserTypedRaw = "0";
            PreserveFormatOnToggle = false;
            lastActionWasPercent = false;
            ClearedExprAfterEqual = false;

            textResult.Font = new Font(textResult.Font.FontFamily, defaultFontSize, textResult.Font.Style);
            textExpression.Font = new Font(textExpression.Font.FontFamily, defaultExpressionFontSize, textExpression.Font.Style);

            AutoFitResultFont();
        }
    }

    /// <summary>アプリ全体で使用する定数</summary>
    internal static class Constants
    {
        internal static class FontSize
        {
            internal const float ERROR_MESSAGE = 20.0f;
            internal const float RESULT_DISPLAY_BASE = 36f;
            internal const float EXPRESSION_DISPLAY_BASE = 10f;
            internal const float MIN_LIMIT = 14f;
            internal const float REDUCTION_STEP = 0.5f;
            internal const float SIZE_EPSILON = 0.1f;
        }

        internal static class Symbol
        {
            internal const string ADD = "+";
            internal const string SUBTRACT = "-";
            internal const string MULTIPLY = "×";
            internal const string DIVIDE = "÷";
            internal const string EQUAL = "=";
        }

        internal static class Numeric
        {
            internal const decimal INITIAL_VALUE = 0m;
            public const string ZERO_VALUE = "0";
            internal const decimal PERCENT_MULTIPLY = 0.01m;

            internal const int MAX_INTEGER_DISPLAY_DIGITS = 16;
            internal const int MAX_FRACTION_DISPLAY_DIGITS_LEADING_ZERO = 17;

            // ★ ここを 16 に引き上げ（固定小数の許容小数桁）
            public const int MAX_TOTAL_FRACTION_DIGITS = 16;

            public const int MAX_SIGNIFICANT_DIGITS = 17;

            // Windows 電卓風の指数切替しきい値
            internal static readonly decimal SCI_SMALL_THRESHOLD = 1e-9m;
            internal static readonly decimal SCI_LARGE_THRESHOLD = 1e16m;
        }

        internal static class ErrorMessage
        {
            internal const string OVERFLOW = "計算範囲を超えました";
            internal const string DIVIDE_BY_ZERO = "0で割ることはできません";
            internal const string UNDEFINED = "結果が定義されていません";
        }

        internal static class SpecialDisplay
        {
            internal const string NEGATE_FUNCTION = "negate";
        }
    }
}
