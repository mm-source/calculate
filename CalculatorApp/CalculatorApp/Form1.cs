using System;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CalculatorApp
{
    public partial class Form1 : Form
    {
        /// <summary>最初の値を保持する変数</summary>
        private decimal FirstValue = 0;

        /// <summary>2番目の値を保持する変数</summary>
        private decimal SecondValue = 0;

        /// <summary>テキストボックスの上書きモードを示すフラグ</summary>
        private bool TextOverwrite = false;

        /// <summary>小数点が入力されているかを示すフラグ</summary>
        private bool NumDot = false;

        /// <summary>加算演算子の記号</summary>
        private const string AddSymbol = "+";

        /// <summary>減算演算子の記号</summary>
        private const string SubtractSymbol = "-";

        /// <summary>乗算演算子の記号</summary>
        private const string MultiplySymbol = "×";

        /// <summary>除算演算子の記号</summary>
        private const string DivideSymbol = "÷";

        /// <summary>等号演算子の記号</summary>
        private const string EqualSymbol = "=";

        /// <summary>初期値:0</summary>
        private const decimal InitialValue = 0m;

        /// <summary>表示値:0</summary>
        private const string ZeroValue = "0";

        /// <summary>％を小数に変換する乗数</summary>
        private const decimal PercentMultiplier = 0.01m;

        /// <summary>エラーメッセージフォントサイズ</summary>
        private const float ErrorFontSize = 20.0f;

        /// <summary>オーバフローが発生したときのエラーメッセージ</summary>
        private const string ErrMsgOverflow = "計算範囲を超えました";

        /// <summary>0除算が発生したときのエラーメッセージ</summary>
        private const string ErrMsgDivZero = "0で割ることはできません";

        /// <summary>0÷0が行われた時のエラーメッセージ</summary>
        private const string ErrMsgUndefined = "結果が定義されていません";

        /// <summary>サインチェンジ表示用関数名</summary>
        private const string NegateFuncName = "negate";

        private bool isNegated = false;

        /// <summary>表示桁数（整数部上限）</summary>
        private const int DisplayMaxIntegerDigits = 16;

        /// <summary>0.から始まる場合の表示桁数</summary>
        private const int DisplayMaxFractionDigitsLeadingZero = 17;

        /// <summary>計算結果表示欄の基準フォントサイズ</summary>
        private const float WinResultBaseSize = 36f;

        /// <summary>途中計算結果欄の基準フォントサイズ</summary>
        private const float WinExprBaseSize = 10f;

        /// <summary>フォントの下限サイズ</summary>
        private const float WinMinFontSize = 14f;

        /// <summary>フォントの縮小幅</summary>
        private const float WinFontStep = 0.5f;

        /// <summary>計算結果表示欄の現在のフォントサイズ</summary>
        private float defaultFontSize;

        /// <summary>途中計算表示欄の現在のフォントサイズ</summary>
        private float defaultExpressionFontSize;

        /// <summary>エラー判定フラグ</summary>
        private bool IsErrorState = false;

        /// <summary>現在の表示内容をクリアして新しい値を入力するかどうか</summary>
        private bool IsClearEntry = false;

        /// <summary>画面に現在表示されている数値</summary>
        private decimal displayValue = InitialValue;

        /// <summary>±時に末端ゼロ/小数点などの見た目を保持するか</summary>
        private bool preserveFormatOnToggle = false;

        /// <summary>入力した生文字列を保持</summary>
        private string lastUserTypedRaw = ZeroValue;

        /// <summary>直前の操作が%だったか</summary>
        private bool lastActionWasPercent = false;

        /// <summary>エラー時に操作無効なキー群</summary>
        private Button[] DisabledButtonsOnError;

        private bool clearedExprAfterEqual = false;

        /// <summary>演算子の種類</summary>
        private enum OperatorType
        {
            NON,
            ADD,
            SUBTRACT,
            MULTIPLY,
            DIVIDE,
            PERCENT
        }

        /// <summary>現在の演算子種別</summary>
        private OperatorType mType = OperatorType.NON;

        /// <summary>フォームのコンストラクタ</summary>
        public Form1()
        {
            InitializeComponent();

            // ディスプレイサイズ固定化
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // エラー時に無効化するボタン
            DisabledButtonsOnError = new Button[]
            {
                btnDot, btnTogglesign, btnPercent, btnPlus,
                btnMinus, btnMultiply, btnDivide, btnEnter
            };
        }

        /// <summary>フォーム初期化</summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            textResult.Text = ZeroValue;
            TextOverwrite = true;

            textResult.Font = new Font(textResult.Font.FontFamily, WinResultBaseSize, textResult.Font.Style);
            textExpression.Font = new Font(textExpression.Font.FontFamily, WinExprBaseSize, textExpression.Font.Style);

            defaultFontSize = textResult.Font.Size;
            defaultExpressionFontSize = textExpression.Font.Size;

            textResult.ReadOnly = true;
            textResult.TextAlign = HorizontalAlignment.Right;
            textResult.BorderStyle = BorderStyle.None;

            textExpression.ReadOnly = true;
            textExpression.TextAlign = HorizontalAlignment.Right;
            textExpression.BorderStyle = BorderStyle.None;
        }

        /// <summary>結果欄のオートフィット</summary>
        private void textResult_TextChanged(object sender, EventArgs e)
        {
            AutoFitResultFont();
        }

        private void textExpression_TextChanged(object sender, EventArgs e)
        {
        }

        /// <summary>数字ボタン</summary>
        private void btnNum_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            if (IsError()) ResetCalculatorState();
            OnDigitButton(btn.Text);
        }

        /// <summary>小数点ボタン</summary>
        private void btnDot_Click(object sender, EventArgs e)
        {
            HandleInitialState();
            OnDotButton();
        }

        /// <summary>演算子ボタン</summary>
        private void btnOperation_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            OperatorType op = OperatorType.NON;

            switch (btn.Text)
            {
                case AddSymbol: op = OperatorType.ADD; break;
                case SubtractSymbol: op = OperatorType.SUBTRACT; break;
                case MultiplySymbol: op = OperatorType.MULTIPLY; break;
                case DivideSymbol: op = OperatorType.DIVIDE; break;
            }
            OnOperatorButton(op);
        }

        /// <summary>＝ボタン</summary>
        private void btnEnter_Click(object sender, EventArgs e)
        {
            OnEqualsButton();
        }

        /// <summary>％ボタン</summary>
        private void btnPercent_Click(object sender, EventArgs e)
        {
            OnPercentButton();
        }

        /// <summary>CE</summary>
        private void btnClearEntry_Click(object sender, EventArgs e)
        {
            OnClearEntryButton();
        }

        /// <summary>C</summary>
        private void btnClear_Click(object sender, EventArgs e)
        {
            OnClearButton();
        }

        /// <summary>Backspace</summary>
        private void btnBack_Click(object sender, EventArgs e)
        {
            OnBackspaceButton();
        }

        /// <summary>±</summary>
        private void btnTogglesign_Click(object sender, EventArgs e)
        {
            OnToggleSignButton();
        }

        /// <summary>最前面</summary>
        private void btnTopMost_Click(object sender, EventArgs e)
        {
            this.TopMost = !this.TopMost;
        }

        // ========= 入力系 =========

        private void OnDigitButton(string digit)
        {
            HandleInitialState();
            SetButtonsEnabled(true);
            lastActionWasPercent = false;

            // 指数表示中は次入力で強制上書き
            if (IsExponentDisplay())
            {
                TextOverwrite = true;
                NumDot = false;
            }

            string current = textResult.Text.Replace(",", "");
            if (!IsInputValid(current, digit)) return;

            if (TextOverwrite) StartNewNumber(digit);
            else AppendDigit(digit);

            UpdateTextResultWithCommas();
            IsClearEntry = false;

            preserveFormatOnToggle = true;
            lastUserTypedRaw = textResult.Text.Replace(",", "");
        }

        private void OnDotButton()
        {
            HandleInitialState();
            lastActionWasPercent = false;

            if (NumDot) return;

            if (TextOverwrite)
            {
                textResult.Text = "0.";
                TextOverwrite = false;
            }
            else
            {
                textResult.Text += ".";
            }
            NumDot = true;

            preserveFormatOnToggle = true;
            lastUserTypedRaw = textResult.Text.Replace(",", "");
        }

        private void OnOperatorButton(OperatorType op)
        {
            if (IsError())
            {
                ResetCalculatorState();
                return;
            }

            try
            {
                if (lastActionWasPercent && mType != OperatorType.NON && !ExpressionEndsWithEqual())
                {
                    decimal cur = GetCurrentValue();
                    PerformPendingCalculation(cur);
                    if (IsError()) return;

                    DisplayNumber(FirstValue, true);
                    mType = op;
                    UpdateExpressionDisplay(FirstValue, mType);

                    lastActionWasPercent = false;
                    TextOverwrite = true;
                    NumDot = false;
                    return;
                }

                if (IsClearEntry)
                {
                    mType = op;
                    UpdateExpressionDisplay(FirstValue, mType);
                    DisplayNumber(FirstValue, true);
                    IsClearEntry = false;
                    return;
                }

                if (TextOverwrite && mType != OperatorType.NON && !ExpressionEndsWithEqual())
                {
                    mType = op;
                    UpdateExpressionDisplay(FirstValue, mType);
                }
                else
                {
                    decimal currentValue = GetCurrentValue();
                    PerformPendingCalculation(currentValue);
                    if (IsError()) return;

                    DisplayNumber(FirstValue, true);
                    mType = op;
                    UpdateExpressionDisplay(FirstValue, mType);
                }

                TextOverwrite = true;
                NumDot = false;
                lastActionWasPercent = false;
                preserveFormatOnToggle = false;
            }
            catch (OverflowException)
            {
                SetErrorState(ErrMsgOverflow);
            }
        }

        private void OnEqualsButton()
        {
            if (ShouldResetOnError()) return;

            try
            {
                decimal result = ProcessEqualsLogic();
                if (IsError()) return;
                DisplayNumber(result, true);

                preserveFormatOnToggle = false;
                lastActionWasPercent = false;
            }
            catch (InvalidOperationException ex)
            {
                SetErrorState(ex.Message);
            }
            catch (OverflowException)
            {
                SetErrorState(ErrMsgOverflow);
            }
        }

        /// <summary>
        /// ％キー：文脈依存。演算子あり時は右辺％（+/- は A*(B/100)、×/÷ は B/100）。
        /// 「＝直後」の特例：R% of R（R*(R/100)）。それ以外の単独％は無効。
        /// </summary>
        private void OnPercentButton()
        {
            if (ShouldResetOnError()) return;

            // ＝直後の特例（例：10+10= → 20、% → 4）
            if (ExpressionEndsWithEqual())
            {
                try
                {
                    decimal r = GetCurrentValue();          // R
                    decimal v = r * CalculatePercent(r);    // R*(R/100)

                    // 計算状態を確定
                    FirstValue = v;
                    SecondValue = InitialValue;
                    mType = OperatorType.NON;

                    // 表示：値のみ（式欄にも "=" を付けない仕様）
                    DisplayNumber(v, true);
                    textExpression.Text = FormatNumberForExpression(v);

                    lastActionWasPercent = true;
                    preserveFormatOnToggle = false;
                    return;
                }
                catch (OverflowException)
                {
                    SetErrorState(ErrMsgOverflow);
                    return;
                }
            }

            // 演算子あり：右辺％
            if (mType != OperatorType.NON)
            {
                decimal rhs = TextOverwrite ? 0m : GetCurrentValue();
                decimal percentValue = CalculatePercent(rhs); // B% = B/100
                UpdatePercentDisplay(percentValue);           // 表示と式の更新

                lastActionWasPercent = true;
                preserveFormatOnToggle = false;
                return;
            }

            // それ以外（単独％）は無効
            return;
        }

        /// <summary>CE：仕様差分の解消</summary>
        private void OnClearEntryButton()
        {
            if (ShouldResetOnError()) return;

            string expr = (textExpression.Text != null ? textExpression.Text.Trim() : string.Empty);

            // 「＝直後」の特別扱い
            if (ExpressionEndsWithEqual())
            {
                // 二項演算の結果（例: "100 + 20 ="）→ CE で全部 0（C と同等の見え方）
                if (HasBinaryOperatorInExpression(expr))
                {
                    ResetCalculatorState();
                    SetButtonsEnabled(true);
                    return;
                }

                // 単独数値の結果（例: "100 ="）→ 式は残し、表示だけ 0
                textResult.Text = ZeroValue;
                TextOverwrite = true;
                NumDot = false;

                FirstValue = InitialValue;
                SecondValue = InitialValue;
                mType = OperatorType.NON;

                preserveFormatOnToggle = false;
                lastUserTypedRaw = ZeroValue;
                lastActionWasPercent = false;
                return;
            }

            // それ以外：現在のエントリのみ 0（式は維持）
            IsClearEntry = true;
            textResult.Text = ZeroValue;
            TextOverwrite = true;
            NumDot = false;

            preserveFormatOnToggle = false;
            lastUserTypedRaw = ZeroValue;
            lastActionWasPercent = false;
        }

        /// <summary>C：全クリア</summary>
        private void OnClearButton()
        {
            ResetAllState();
        }

        /// <summary>Backspace：末尾1文字削除</summary>
        private void OnBackspaceButton()
        {
            if (ShouldResetOnError()) return;

            if (ExpressionEndsWithEqual())
            {
                textExpression.Text = "";
                TextOverwrite = true;
                NumDot = false;
                preserveFormatOnToggle = false;
                lastActionWasPercent = false;
                clearedExprAfterEqual = true;
                return;
            }

            if (clearedExprAfterEqual) return;

            // 指数表示中は新規入力開始扱い
            if (IsExponentDisplay())
            {
                TextOverwrite = true;
                NumDot = false;
                textResult.Text = ZeroValue;

                preserveFormatOnToggle = false;
                lastUserTypedRaw = ZeroValue;
                lastActionWasPercent = false;
                return;
            }

            if (TextOverwrite) return;

            HandleBackspace();
            UpdateTextResultWithCommas();

            preserveFormatOnToggle = true;
            lastUserTypedRaw = textResult.Text.Replace(",", "");
            lastActionWasPercent = false;
        }

        /// <summary>±の処理（見た目保持／=直後は negate(...) 表示）</summary>
        private void OnToggleSignButton()
        {
            if (ShouldResetOnError()) return;
            if (string.IsNullOrEmpty(textResult.Text)) return;

            if (preserveFormatOnToggle && !IsExponentDisplay())
            {
                string raw = textResult.Text.Replace(",", "");
                raw = ToggleSignRaw(raw);
                SetTextFromRawPreservingCommas(raw);

                TextOverwrite = false;
                NumDot = (raw.IndexOf('.') >= 0);

                UpdateExpressionForToggleSign();

                lastUserTypedRaw = raw;
                lastActionWasPercent = false;
                return;
            }

            // 計算表示のときは数値反転
            ToggleSign();
            UpdateExpressionForToggleSign();

            preserveFormatOnToggle = false;
            lastActionWasPercent = false;
        }

        // ========= ヘルパー =========

        private void StartNewNumber(string digit)
        {
            textResult.Text = digit;
            TextOverwrite = false;
            NumDot = (digit == ".");
        }

        private void AppendDigit(string digit)
        {
            textResult.Text += digit;
        }

        private bool IsInputValid(string currentText, string digit)
        {
            bool startsWithZeroDot = currentText.StartsWith("0.") || currentText.StartsWith("-0.");
            int maxDigits = startsWithZeroDot ? DisplayMaxFractionDigitsLeadingZero : DisplayMaxIntegerDigits;

            string nextText = TextOverwrite ? digit : currentText + digit;
            int nextLength = nextText.Replace(".", "").Replace("-", "").Length;

            if (nextLength > maxDigits) return false;

            if (!TextOverwrite && currentText == ZeroValue && digit == ZeroValue && !NumDot) return false;

            return true;
        }

        private string GetOperatorSymbol(OperatorType type)
        {
            switch (type)
            {
                case OperatorType.ADD: return AddSymbol;
                case OperatorType.SUBTRACT: return SubtractSymbol;
                case OperatorType.MULTIPLY: return MultiplySymbol;
                case OperatorType.DIVIDE: return DivideSymbol;
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
            if (ExpressionEndsWithEqual() || mType == OperatorType.NON)
            {
                FirstValue = currentValue;
            }
            else
            {
                if (mType == OperatorType.DIVIDE && currentValue == InitialValue)
                {
                    if (FirstValue == InitialValue) { SetErrorState(ErrMsgUndefined); }
                    else { SetErrorState(ErrMsgDivZero); }
                    return;
                }

                decimal result = Calculate(FirstValue, currentValue, mType);
                FirstValue = result;
            }
        }

        /// <summary>途中式表示更新（negate(...) の維持に対応）</summary>
        private void UpdateExpressionDisplay(decimal value, OperatorType type)
        {
            string op = GetOperatorSymbol(type);
            string currentExpr = (textExpression.Text != null ? textExpression.Text.Trim() : string.Empty);

            if (!string.IsNullOrEmpty(currentExpr) &&
                !currentExpr.EndsWith(EqualSymbol) &&
                currentExpr.StartsWith(NegateFuncName + "(", StringComparison.Ordinal))
            {
                textExpression.Text = currentExpr + " " + op;
                return;
            }

            textExpression.Text = string.Format("{0} {1}", FormatNumberForExpression(value), op);
        }

        /// <summary>＝処理本体（negate(...) を維持したまま右辺と = を付ける）</summary>
        private decimal ProcessEqualsLogic()
        {
            decimal currentValue = GetCurrentValue();
            bool isFirstEqual = !ExpressionEndsWithEqual();

            if (mType == OperatorType.NON)
            {
                SecondValue = currentValue;
                textExpression.Text = string.Format("{0} {1}",
                    FormatNumberForExpression(currentValue), EqualSymbol);
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

            if (mType == OperatorType.DIVIDE && right == InitialValue)
            {
                if (left == InitialValue) throw new InvalidOperationException(ErrMsgUndefined);
                else throw new InvalidOperationException(ErrMsgDivZero);
            }

            decimal result = Calculate(left, right, mType);
            FirstValue = result;

            string opSym = GetOperatorSymbol(mType);
            string leftExpr = FormatNumberForExpression(left);
            string rightExpr = FormatNumberForExpression(right);

            string curr = (textExpression.Text != null ? textExpression.Text.Trim() : string.Empty);
            if (!string.IsNullOrEmpty(curr) &&
                !curr.EndsWith(EqualSymbol) &&
                curr.StartsWith(NegateFuncName + "(", StringComparison.Ordinal))
            {
                if (curr.EndsWith(opSym))
                {
                    textExpression.Text = curr + " " + rightExpr + " " + EqualSymbol;
                }
                else
                {
                    textExpression.Text = curr + " " + EqualSymbol;
                }
            }
            else
            {
                textExpression.Text = string.Format("{0} {1} {2} {3}",
                    leftExpr, opSym, rightExpr, EqualSymbol);
            }

            return result;
        }

        private decimal CalculatePercent(decimal value)
        {
            return value * PercentMultiplier;
        }

        /// <summary>％の表示と式更新（+/- は A*(B/100)、×/÷ は B/100）</summary>
        private void UpdatePercentDisplay(decimal percentValue)
        {
            decimal previousValue = FirstValue;
            decimal calculatedValue;

            if (mType == OperatorType.ADD || mType == OperatorType.SUBTRACT)
            {
                calculatedValue = previousValue * percentValue; // A*(B/100)
                textExpression.Text = string.Format("{0} {1} {2}",
                    FormatNumberForExpression(previousValue),
                    GetOperatorSymbol(mType),
                    FormatNumberForExpression(calculatedValue));
            }
            else
            {
                calculatedValue = percentValue; // B/100
                textExpression.Text = string.Format("{0} {1} {2}",
                    FormatNumberForExpression(previousValue),
                    GetOperatorSymbol(mType),
                    FormatNumberForExpression(calculatedValue));
            }

            DisplayNumber(calculatedValue, false);
        }

        private void HandleBackspace()
        {
            string currentText = textResult.Text.Replace(",", "");
            if (currentText.Length > 0)
            {
                string newText = currentText.Substring(0, currentText.Length - 1);

                if (string.IsNullOrEmpty(newText) || newText == "-")
                {
                    textResult.Text = ZeroValue;
                    TextOverwrite = true;
                    NumDot = false;
                }
                else
                {
                    textResult.Text = newText;
                    NumDot = textResult.Text.Contains(".");
                }
            }
            else
            {
                ResetCalculatorState();
            }
        }

        private void ToggleSign()
        {
            displayValue = GetCurrentValue();
            displayValue = -displayValue;
            DisplayNumber(displayValue, false);
            isNegated = true;
        }

        private string ToggleSignRaw(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return raw;
            if (raw[0] == '-') return raw.Substring(1);
            return "-" + raw;
        }

        private void SetTextFromRawPreservingCommas(string raw)
        {
            textResult.Text = raw;
            UpdateTextResultWithCommas();
        }

        /// <summary>＝直後の ±：negate(...) 表示（= なし）、入れ子も対応</summary>
        private void UpdateExpressionForToggleSign()
        {
            string expr = (textExpression.Text != null ? textExpression.Text.Trim() : string.Empty);

            if (expr.EndsWith(EqualSymbol))
            {
                int eq = expr.LastIndexOf(EqualSymbol);
                string body = (eq >= 0 ? expr.Substring(0, eq) : expr).Trim();
                if (string.IsNullOrEmpty(body)) body = FormatNumberForExpression(FirstValue);

                textExpression.Text = NegateFuncName + "(" + body + ")";
                return;
            }

            if (expr.StartsWith(NegateFuncName + "(", StringComparison.Ordinal))
            {
                textExpression.Text = NegateFuncName + "(" + expr + ")";
            }
        }

        private void DisplayNumber(decimal value, bool overwrite)
        {
            decimal rounded = RoundResult(value);
            textResult.Text = FormatNumberForDisplay(rounded);
            UpdateTextResultWithCommas();
            TextOverwrite = overwrite;
            NumDot = false;

            preserveFormatOnToggle = false;
            lastUserTypedRaw = textResult.Text.Replace(",", "");
        }

        private void UpdateTextResultWithCommas()
        {
            if (IsError()) return;
            if (IsExponentDisplay()) return;

            string currentText = textResult.Text.Replace(",", "");
            if (string.IsNullOrEmpty(currentText) || currentText == "-" || (currentText == "0" && !NumDot)) return;

            bool isNegative = currentText.StartsWith("-");
            if (isNegative) currentText = currentText.Substring(1);

            int dotIndex = currentText.IndexOf('.');
            string integerPart = currentText;
            string decimalPart = "";

            if (dotIndex != -1)
            {
                integerPart = currentText.Substring(0, dotIndex);
                decimalPart = currentText.Substring(dotIndex + 1);
            }

            try
            {
                decimal integerValue;
                if (decimal.TryParse(integerPart, NumberStyles.Number, CultureInfo.InvariantCulture, out integerValue))
                {
                    string formattedInteger = integerValue.ToString("#,##0", CultureInfo.InvariantCulture);
                    string newText = formattedInteger;

                    if (dotIndex != -1) newText += "." + decimalPart;
                    if (isNegative) newText = "-" + newText;

                    textResult.Text = newText;
                }
            }
            catch (FormatException)
            {
            }
        }

        private string FormatNumberForExpression(decimal value)
        {
            return FormatNumberForDisplay(value);
        }

        private string FormatExponential(decimal value)
        {
            string gFormat = value.ToString("G15", CultureInfo.InvariantCulture);
            string expString = gFormat.Contains("E")
                ? gFormat
                : decimal.Parse(gFormat, CultureInfo.InvariantCulture).ToString("E", CultureInfo.InvariantCulture);

            expString = expString.Replace("E+", "e+").Replace("E-", "e-");
            string[] parts = expString.Split(new char[] { 'e' });

            string mantissa = parts[0].TrimEnd('0');
            if (mantissa.EndsWith(".")) mantissa = mantissa.TrimEnd('.');
            if (!mantissa.Contains(".")) mantissa += ".";

            string exponent = Regex.Replace(parts[1], @"^(\+|-)(0)(\d+)", "$1$3");
            return mantissa + "e" + exponent;
        }

        private string FormatNumberForDisplay(decimal value)
        {
            decimal abs = Math.Abs(value);
            if (abs == 0m) return ZeroValue;

            string fixedStr = value.ToString("0.#############################", CultureInfo.InvariantCulture);

            if (abs >= 1m)
            {
                int dot = fixedStr.IndexOf('.');
                bool neg = (fixedStr[0] == '-');
                int intLen = (dot >= 0 ? dot : fixedStr.Length) - (neg ? 1 : 0);

                if (intLen > DisplayMaxIntegerDigits) return FormatExponential(value);
                return fixedStr;
            }
            else
            {
                int dot = fixedStr.IndexOf('.');
                int fracLen = (dot >= 0) ? (fixedStr.Length - dot - 1) : 0;

                if (fracLen > DisplayMaxFractionDigitsLeadingZero) return FormatExponential(value);
                return fixedStr;
            }
        }

        private decimal GetCurrentValue()
        {
            return ParseDisplayToDecimal(textResult.Text);
        }

        private decimal ParseDisplayToDecimal(string text)
        {
            if (string.IsNullOrEmpty(text)) return InitialValue;

            string s = text.Replace(",", "");
            decimal dv;

            if (s.IndexOf('e') >= 0 || s.IndexOf('E') >= 0)
            {
                double dd;
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out dd))
                {
                    try { return (decimal)dd; } catch { return InitialValue; }
                }
                return InitialValue;
            }

            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out dv)) return dv;
            return InitialValue;
        }

        private decimal RoundResult(decimal value)
        {
            decimal abs = Math.Abs(value);

            if (abs > 0m && abs < 1m)
            {
                return Math.Round(value, 17, MidpointRounding.AwayFromZero);
            }

            if (abs >= 1m)
            {
                string integerPartStr = Math.Floor(abs).ToString(CultureInfo.InvariantCulture);
                int integerLength = integerPartStr.Length;

                int decimalPlacesToRound = 16 - integerLength;
                if (decimalPlacesToRound >= 0)
                {
                    return Math.Round(value, decimalPlacesToRound, MidpointRounding.AwayFromZero);
                }
            }

            return value;
        }

        private bool IsError()
        {
            return IsErrorState;
        }

        private bool ExpressionEndsWithEqual()
        {
            return textExpression.Text.Length > 0 && textExpression.Text.EndsWith(EqualSymbol);
        }

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

            while (size > WinMinFontSize)
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
                size -= WinFontStep;
            }

            if (Math.Abs(textResult.Font.Size - WinMinFontSize) > 0.1f)
            {
                Font oldFinal = textResult.Font;
                textResult.Font = new Font(family, WinMinFontSize, style);
                oldFinal.Dispose();
            }
        }

        private void SetButtonsEnabled(bool enabled)
        {
            foreach (Button btn in DisabledButtonsOnError)
            {
                btn.Enabled = enabled;
            }
        }

        private void SetErrorState(string message)
        {
            textResult.Text = message;

            float sz = ErrorFontSize;
            if (sz < WinMinFontSize) sz = WinMinFontSize;
            textResult.Font = new Font(textResult.Font.FontFamily, sz, textResult.Font.Style);

            IsErrorState = true;
            SetButtonsEnabled(false);
        }

        private void HandleInitialState()
        {
            if (IsErrorState || ExpressionEndsWithEqual())
            {
                ResetCalculatorState();
                clearedExprAfterEqual = false;
            }
        }

        private void ResetAllState()
        {
            ResetCalculatorState();
            SetButtonsEnabled(true);
        }

        private void ResetCalculatorState()
        {
            FirstValue = InitialValue;
            SecondValue = InitialValue;
            mType = OperatorType.NON;
            textExpression.Text = "";
            textResult.Text = ZeroValue;
            TextOverwrite = true;
            NumDot = false;
            IsErrorState = false;
            IsClearEntry = false;
            displayValue = InitialValue;

            preserveFormatOnToggle = false;
            lastUserTypedRaw = ZeroValue;
            lastActionWasPercent = false;
            clearedExprAfterEqual = false;

            textResult.Font = new Font(textResult.Font.FontFamily, defaultFontSize, textResult.Font.Style);
            textExpression.Font = new Font(textExpression.Font.FontFamily, defaultExpressionFontSize, textExpression.Font.Style);

            AutoFitResultFont();
        }

        // ========== 追加ヘルパー：式に二項演算子が含まれるか ==========
        private bool HasBinaryOperatorInExpression(string expr)
        {
            if (string.IsNullOrEmpty(expr)) return false;
            int eq = expr.LastIndexOf(EqualSymbol);
            string body = (eq >= 0 ? expr.Substring(0, eq) : expr);

            if (body.IndexOf(" " + AddSymbol + " ", StringComparison.Ordinal) >= 0) return true;
            if (body.IndexOf(" " + SubtractSymbol + " ", StringComparison.Ordinal) >= 0) return true;
            if (body.IndexOf(" " + MultiplySymbol + " ", StringComparison.Ordinal) >= 0) return true;
            if (body.IndexOf(" " + DivideSymbol + " ", StringComparison.Ordinal) >= 0) return true;

            return false;
        }
    }
}
