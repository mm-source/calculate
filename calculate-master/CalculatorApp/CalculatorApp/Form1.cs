using System;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CalculatorApp
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 最初の値を保持する変数である。
        /// </summary>
        private decimal firstValue = InitialValue;

        /// <summary>
        /// 2番目の値を保持する変数である。
        /// </summary>
        private decimal secondValue = InitialValue;

        /// <summary>
        /// テキストボックスの上書きモードを示すフラグである。
        /// </summary>
        private bool text_overwrite = false;

        /// <summary>
        /// 小数点が入力されているかを示すフラグである。
        /// </summary>
        private bool Num_Dot = false;

        /// <summary>加算演算子の記号である。</summary>
        private const string AddSymbol = "+";
        /// <summary>減算演算子の記号である。</summary>
        private const string SubtractSymbol = "-";
        /// <summary>乗算演算子の記号である。</summary>
        private const string MultiplySymbol = "×";
        /// <summary>除算演算子の記号である。</summary>
        private const string DivideSymbol = "÷";
        /// <summary>等号演算子の記号である。</summary>
        private const string EqualSymbol = "=";

        /// <summary>初期値である（0）。</summary>
        private const decimal InitialValue = 0m;
        /// <summary>表示値のゼロ文字列である。</summary>
        private const string ZeroValue = "0";
        /// <summary>％を小数に変換する乗数である。</summary>
        private const decimal PercentMultiplier = 0.01m;

        // ===== メッセージ／数値表示ポリシー =====
        private const float ErrorFontSize = 20.0f;
        private const string ErrMsgOverflow = "計算範囲を超えました";
        private const string ErrMsgDivByZero = "0で割ることはできません";
        private const string ErrMsgUndefined = "結果が定義されていません"; // 0÷0 用
        private const string NegateFuncName = "negate";

        // 整数部：16桁まで十進表示、超えたら指数表記
        private const int DisplayMaxIntegerDigits = 16;
        // 0.～：小数部17桁まで十進表示、超えたら指数表記
        private const int DisplayMaxFractionDigitsLeadingZero = 17;

        // 入力桁制御
        private const int MaxDigitsDefault = 16;
        private const int MaxDigitsWithLeadingZero = 17;

        // --- Windows 風フォントとサイズ ---
        private const float WinResultBaseSize = 36f;   // 結果欄の基準サイズ（大）
        private const float WinExprBaseSize = 10f;     // 式欄の固定サイズ（小）
        private const float WinMinFontSize = 14f;      // 縮小の下限
        private const float WinFontStep = 0.5f;        // 縮小ステップ

        // 優先フォント（OSにあるものから選択）
        private static readonly string[] WinResultPreferredFonts = { "Segoe UI Semibold", "Segoe UI", "Meiryo UI" };
        private static readonly string[] WinExpressionPreferredFonts = { "Segoe UI", "Meiryo UI", "MS UI Gothic" };

        private float defaultFontSize;              // 結果欄：基準フォントサイズ
        private float defaultExpressionFontSize;    // 式欄：固定フォントサイズ
        private bool isErrorState = false;
        private bool isClearEntry = false;
        private decimal displayValue = InitialValue;

        // ±で末尾ゼロを保持するための状態
        private bool preserveFormatOnToggle = false;      // 直前がユーザー入力なら true
        private string lastUserTypedRaw = ZeroValue;      // カンマ無しの生文字列

        // % 直後の演算子押下で確定計算するための状態
        private bool lastActionWasPercent = false;

        // ツールチップ（式全文表示用）
        private ToolTip expressionToolTip;

        // エラー時に操作無効なキー（C 以外）
        private Button[] DisabledButtonsOnError;

        // ＝直後 Backspace で式だけ消した状態を覚える
        private bool clearedExprAfterEqual = false;

        private enum OperatorType
        {
            /// <summary>演算なしである。</summary>
            NON,
            /// <summary>加算である。</summary>
            ADD,
            /// <summary>減算である。</summary>
            SUBTRACT,
            /// <summary>乗算である。</summary>
            MULTIPLY,
            /// <summary>除算である。</summary>
            DIVIDE,
            /// <summary>パーセントである。</summary>
            PERCENT
        }

        /// <summary>現在の演算子種別を保持する変数である。</summary>
        private OperatorType mType = OperatorType.NON;

        /// <summary>
        /// コンストラクタである。フォーム外観を固定し、エラー時に無効化するボタン群を初期化する。
        /// </summary>
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

        /// <summary>
        /// フォーム読み込み時の初期化処理である。
        /// フォント設定、読み取り専用、右揃え、ツールチップ、初期フォントフィットを実施する。
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            textResult.Text = ZeroValue;
            text_overwrite = true;

            // 結果欄フォント：Windows風（Semibold 優先・Bold）
            Font resFont = CreateFirstAvailableFont(
                WinResultPreferredFonts,
                WinResultBaseSize,
                FontStyle.Bold,
                textResult.Font);
            textResult.Font = resFont;

            // 式欄フォント：Windows風（固定サイズ）
            Font exprFont = CreateFirstAvailableFont(
                WinExpressionPreferredFonts,
                WinExprBaseSize,
                FontStyle.Regular,
                textExpression.Font);
            textExpression.Font = exprFont;

            // 基準サイズの保持
            defaultFontSize = textResult.Font.Size;
            defaultExpressionFontSize = textExpression.Font.Size;

            // 表示欄の共通設定
            textResult.ReadOnly = true;
            textResult.TextAlign = HorizontalAlignment.Right;
            textResult.BorderStyle = BorderStyle.None;

            textExpression.ReadOnly = true;
            textExpression.TextAlign = HorizontalAlignment.Right;
            textExpression.BorderStyle = BorderStyle.None;

            // ツールチップ
            expressionToolTip = new ToolTip();
            expressionToolTip.InitialDelay = 300;
            expressionToolTip.ReshowDelay = 100;
            expressionToolTip.AutoPopDelay = 10000;
            expressionToolTip.ShowAlways = true;

            // 初期フィット（結果欄のみ）
            AutoFitResultFontWindowsLike();
        }

        /// <summary>
        /// テーブルレイアウトのペイントイベントである（拡張用の空実装である）。
        /// </summary>
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
        }

        /// <summary>
        /// 結果表示テキスト変更時に、フォントを枠内に収めるよう“縮小のみ”で調整する。
        /// </summary>
        private void textResult_TextChanged(object sender, EventArgs e)
        {
            AutoFitResultFontWindowsLike();
        }

        /// <summary>
        /// 式表示テキスト変更時に、末尾へ自動スクロールし、ツールチップへ全文を設定する。
        /// </summary>
        private void textExpression_TextChanged(object sender, EventArgs e)
        {
            AutoScrollExpressionToEnd();
            if (expressionToolTip != null)
            {
                expressionToolTip.SetToolTip(textExpression, textExpression.Text);
            }
        }

        /// <summary>
        /// 数字ボタン押下時のイベントハンドラである。OnDigitButton へ委譲する。
        /// </summary>
        private void btnNum_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null)
            {
                return;
            }
            if (IsError())
            {
                ResetCalculatorState();
            }
            OnDigitButton(btn.Text);
        }

        /// <summary>
        /// 小数点ボタン押下時のイベントハンドラである。OnDotButton へ委譲する。
        /// </summary>
        private void btnDot_Click(object sender, EventArgs e)
        {
            HandleInitialState();
            OnDotButton();
        }

        /// <summary>
        /// 演算子ボタン押下時のイベントハンドラである。表示記号から列挙へ変換し OnOperatorButton へ委譲する。
        /// </summary>
        private void btnOperation_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            OperatorType op = OperatorType.NON;

            switch (btn.Text)
            {
                case AddSymbol:
                    {
                        op = OperatorType.ADD;
                        break;
                    }
                case SubtractSymbol:
                    {
                        op = OperatorType.SUBTRACT;
                        break;
                    }
                case MultiplySymbol:
                    {
                        op = OperatorType.MULTIPLY;
                        break;
                    }
                case DivideSymbol:
                    {
                        op = OperatorType.DIVIDE;
                        break;
                    }
            }
            OnOperatorButton(op);
        }

        /// <summary>
        /// ＝（イコール）押下時のイベントハンドラである。OnEqualsButton へ委譲する。
        /// </summary>
        private void btnEnter_Click(object sender, EventArgs e)
        {
            OnEqualsButton();
        }

        /// <summary>
        /// ％押下時のイベントハンドラである。OnPercentButton へ委譲する。
        /// </summary>
        private void btnPercent_Click(object sender, EventArgs e)
        {
            OnPercentButton();
        }

        /// <summary>
        /// CE（クリアエントリ）押下時のイベントハンドラである。OnClearEntryButton へ委譲する。
        /// </summary>
        private void btnClearEntry_Click(object sender, EventArgs e)
        {
            OnClearEntryButton();
        }

        /// <summary>
        /// C（全クリア）押下時のイベントハンドラである。OnClearButton へ委譲する。
        /// </summary>
        private void btnClear_Click(object sender, EventArgs e)
        {
            OnClearButton();
        }

        /// <summary>
        /// Backspace 押下時のイベントハンドラである。OnBackspaceButton へ委譲する。
        /// </summary>
        private void btnBack_Click(object sender, EventArgs e)
        {
            OnBackspaceButton();
        }

        /// <summary>
        /// ±（符号反転）押下時のイベントハンドラである。OnToggleSignButton へ委譲する。
        /// </summary>
        private void btnTogglesign_Click(object sender, EventArgs e)
        {
            OnToggleSignButton();
        }

        /// <summary>
        /// ウィンドウの最前面表示設定をトグルする。
        /// </summary>
        private void btnTopMost_Click(object sender, EventArgs e)
        {
            this.TopMost = !this.TopMost;
        }

        // ================= On 系（UI 入口 → ロジック） =================

        /// <summary>
        /// 数字入力のメイン処理である。エラー復帰、指数表示からの上書き開始、桁数制御、カンマ付与、
        /// 「±で末尾ゼロ保持」用の生文字列更新を行う。
        /// </summary>
        private void OnDigitButton(string digit)
        {
            HandleInitialState();
            SetButtonsEnabled(true);
            lastActionWasPercent = false;

            // 指数表示中は次入力で強制上書き
            if (IsExponentDisplay())
            {
                text_overwrite = true;
                Num_Dot = false;
            }

            string current = textResult.Text.Replace(",", "");
            if (!IsInputValid(current, digit))
            {
                return;
            }

            if (text_overwrite)
            {
                StartNewNumber(digit);
            }
            else
            {
                AppendDigit(digit);
            }

            UpdateTextResultWithCommas();
            isClearEntry = false;

            // ユーザー入力なので、±で末尾ゼロ保持を有効化
            preserveFormatOnToggle = true;
            lastUserTypedRaw = textResult.Text.Replace(",", "");
        }

        /// <summary>
        /// 小数点入力のメイン処理である。0. からの開始、重複小数点抑止、
        /// 「±で末尾ゼロ保持」用生文字列の更新を行う。
        /// </summary>
        private void OnDotButton()
        {
            HandleInitialState();
            lastActionWasPercent = false;

            if (Num_Dot)
            {
                return;
            }

            if (text_overwrite)
            {
                textResult.Text = "0.";
                text_overwrite = false;
            }
            else
            {
                textResult.Text += ".";
            }
            Num_Dot = true;

            preserveFormatOnToggle = true;
            lastUserTypedRaw = textResult.Text.Replace(",", "");
        }

        /// <summary>
        /// 演算子押下時のメイン処理である。保留演算の解決、%直後確定、連続演算子の上書き、
        /// 途中式の更新、状態遷移を行う。
        /// </summary>
        private void OnOperatorButton(OperatorType op)
        {
            if (IsError())
            {
                ResetCalculatorState();
                return;
            }

            try
            {
                // 直前が % の場合：ここで確定計算してから新しい演算子へ（Windows準拠）
                if (lastActionWasPercent && mType != OperatorType.NON && !ExpressionEndsWithEqual())
                {
                    decimal cur = GetCurrentValue();
                    PerformPendingCalculation(cur);
                    if (IsError())
                    {
                        return;
                    }

                    DisplayNumber(firstValue, true);
                    mType = op;
                    UpdateExpressionDisplay(firstValue, mType);

                    lastActionWasPercent = false;
                    text_overwrite = true;
                    Num_Dot = false;
                    return;
                }

                if (isClearEntry)
                {
                    mType = op;
                    UpdateExpressionDisplay(firstValue, mType);
                    DisplayNumber(firstValue, true);
                    isClearEntry = false;
                    return;
                }

                // 連続演算子
                if (text_overwrite && mType != OperatorType.NON && !ExpressionEndsWithEqual())
                {
                    mType = op;
                    UpdateExpressionDisplay(firstValue, mType);
                }
                else
                {
                    decimal currentValue = GetCurrentValue();
                    PerformPendingCalculation(currentValue);
                    if (IsError())
                    {
                        return;
                    }

                    DisplayNumber(firstValue, true);
                    mType = op;
                    UpdateExpressionDisplay(firstValue, mType);
                }

                text_overwrite = true;
                Num_Dot = false;
                lastActionWasPercent = false;
                preserveFormatOnToggle = false; // 計算結果表示に変化
            }
            catch (OverflowException)
            {
                SetErrorState(ErrMsgOverflow);
            }
        }

        /// <summary>
        /// ＝（確定）押下時のメイン処理である。初回/連続＝の双方に対応し、途中式と結果を更新する。
        /// 0÷0/÷0 は規定メッセージでエラー化する。
        /// </summary>
        private void OnEqualsButton()
        {
            if (ShouldResetOnError())
            {
                return;
            }

            try
            {
                decimal result = ProcessEqualsLogic();
                if (IsError())
                {
                    return;
                }
                DisplayNumber(result, true);

                // ＝確定後はユーザー入力ではない
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
        /// ％押下時のメイン処理である。単項%（/100）と、演算子右辺%（加減は A*(B/100)、乗除は B/100）に対応する。
        /// 直後は演算子押下で確定するようフラグを設定する。
        /// </summary>
        private void OnPercentButton()
        {
            if (ShouldResetOnError())
            {
                return;
            }

            // mType == NON でも /100 の単項%（Windows準拠）
            if (mType == OperatorType.NON)
            {
                decimal v = GetCurrentValue() * PercentMultiplier;
                // BS で編集できるよう overwrite=false
                DisplayNumber(v, false);

                lastActionWasPercent = true;
                preserveFormatOnToggle = false;
                return;
            }

            decimal currentValue = GetCurrentValue();
            decimal percentValue = CalculatePercent(currentValue);
            UpdatePercentDisplay(percentValue);

            // % 直後フラグ ON（演算子押下で確定）
            lastActionWasPercent = true;
            preserveFormatOnToggle = false;
        }

        /// <summary>
        /// CE（表示のみクリア）押下時の処理である。式は維持し、表示を 0 に戻す。
        /// </summary>
        private void OnClearEntryButton()
        {
            if (ShouldResetOnError())
            {
                return;
            }

            isClearEntry = true;
            textResult.Text = ZeroValue;
            text_overwrite = true;
            Num_Dot = false;

            preserveFormatOnToggle = false;
            lastUserTypedRaw = ZeroValue;
            lastActionWasPercent = false;
        }

        /// <summary>
        /// C（全クリア）押下時の処理である。全状態を初期化する。
        /// </summary>
        private void OnClearButton()
        {
            ResetAllState();
        }

        /// <summary>
        /// Backspace 押下時の処理である。＝直後は式だけ消去、指数表示中は新規入力開始、
        /// 通常は末尾1文字削除を行う。
        /// </summary>
        private void OnBackspaceButton()
        {
            if (ShouldResetOnError())
            {
                return;
            }

            // ＝直後の Backspace：式だけ消して結果は保持（Windows準拠）
            if (ExpressionEndsWithEqual())
            {
                textExpression.Text = "";
                text_overwrite = true;      // 次の数字入力は新規開始（上書き）
                Num_Dot = false;
                preserveFormatOnToggle = false;
                lastActionWasPercent = false;
                clearedExprAfterEqual = true; // 「式だけ消した」状態
                return;                       // ★ 結果は削らない
            }

            // 直後にもう一度 Backspace は無効（Windows準拠）
            if (clearedExprAfterEqual)
            {
                return;
            }

            // 指数表示中は新規入力開始扱い
            if (IsExponentDisplay())
            {
                text_overwrite = true;
                Num_Dot = false;
                textResult.Text = ZeroValue;

                preserveFormatOnToggle = false;
                lastUserTypedRaw = ZeroValue;
                lastActionWasPercent = false;
                return;
            }

            if (text_overwrite)
            {
                return;
            }

            HandleBackspace();
            UpdateTextResultWithCommas();

            // ユーザー編集継続なので保持
            preserveFormatOnToggle = true;
            lastUserTypedRaw = textResult.Text.Replace(",", "");
            lastActionWasPercent = false;
        }

        /// <summary>
        /// ±（符号反転）押下時の処理である。ユーザー入力の見た目保持（末尾ゼロ維持）と、
        /// ＝直後の negate(...) 入れ子表記更新に対応する。
        /// </summary>
        private void OnToggleSignButton()
        {
            if (ShouldResetOnError())
            {
                return;
            }

            if (string.IsNullOrEmpty(textResult.Text))
            {
                return;
            }

            // ユーザー入力の“見た目”を維持したまま ±（指数は数値反転）
            if (preserveFormatOnToggle && !IsExponentDisplay())
            {
                string raw = textResult.Text.Replace(",", "");
                raw = ToggleSignRaw(raw);               // 文字列で符号反転
                SetTextFromRawPreservingCommas(raw);    // カンマ付与

                // 編集継続前提
                text_overwrite = false;
                Num_Dot = (raw.IndexOf('.') >= 0);

                // ＝直後は negate(...) の入れ子表現
                UpdateExpressionForToggleSign();

                // 連続±でも保持
                lastUserTypedRaw = raw;
                lastActionWasPercent = false;
                return;
            }

            // 計算表示のときは数値反転
            ToggleSign();                  // DisplayNumber 経由
            UpdateExpressionForToggleSign();

            preserveFormatOnToggle = false;
            lastActionWasPercent = false;
        }

        // ================= 入力ユーティリティ =================

        /// <summary>
        /// 新しい数値入力を開始する。上書きモードを解除し、小数点フラグを更新する。
        /// </summary>
        private void StartNewNumber(string digit)
        {
            textResult.Text = digit;
            text_overwrite = false;
            Num_Dot = (digit == ".");
        }

        /// <summary>
        /// 既存の入力の末尾に数字を追加する。
        /// </summary>
        private void AppendDigit(string digit)
        {
            textResult.Text += digit;
        }

        /// <summary>
        /// 入力の妥当性を検証する。整数/小数の最大桁数、先頭0の扱い、二重0の抑止などを行う。
        /// </summary>
        private bool IsInputValid(string currentText, string digit)
        {
            bool startsWithZeroDot = currentText.StartsWith("0.") || currentText.StartsWith("-0.");
            int maxDigits = startsWithZeroDot ? MaxDigitsWithLeadingZero : MaxDigitsDefault;

            string nextText = text_overwrite ? digit : currentText + digit;
            int nextLength = nextText.Replace(".", "").Replace("-", "").Length;

            if (nextLength > maxDigits)
            {
                return false;
            }

            if (!text_overwrite && currentText == ZeroValue && digit == ZeroValue && !Num_Dot)
            {
                return false;
            }

            return true;
        }

        // ================= 計算／式更新 =================

        /// <summary>
        /// 指定された演算子タイプに基づき、演算子の記号を返す。
        /// </summary>
        /// <param name="type">演算子のタイプである。</param>
        /// <returns>指定された演算子タイプに対応する記号である。</returns>
        private string GetOperatorSymbol(OperatorType type)
        {
            switch (type)
            {
                case OperatorType.ADD:
                    {
                        return AddSymbol;
                    }
                case OperatorType.SUBTRACT:
                    {
                        return SubtractSymbol;
                    }
                case OperatorType.MULTIPLY:
                    {
                        return MultiplySymbol;
                    }
                case OperatorType.DIVIDE:
                    {
                        return DivideSymbol;
                    }
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        /// <summary>
        /// 左右と演算子種別から結果を計算する（÷0 の例外処理は呼び出し側で行う）。
        /// </summary>
        private decimal Calculate(decimal left, decimal right, OperatorType type)
        {
            switch (type)
            {
                case OperatorType.ADD:
                    {
                        return left + right;
                    }
                case OperatorType.SUBTRACT:
                    {
                        return left - right;
                    }
                case OperatorType.MULTIPLY:
                    {
                        return left * right;
                    }
                case OperatorType.DIVIDE:
                    {
                        return left / right;
                    }
                default:
                    {
                        return right;
                    }
            }
        }

        /// <summary>
        /// 保留中の演算を解決する。未選択なら左辺を現在値に更新し、÷0/0÷0 はここでエラー化する。
        /// </summary>
        private void PerformPendingCalculation(decimal currentValue)
        {
            if (ExpressionEndsWithEqual() || mType == OperatorType.NON)
            {
                firstValue = currentValue;
            }
            else
            {
                if (mType == OperatorType.DIVIDE && currentValue == InitialValue)
                {
                    // 0÷0 は未定義、その他 ÷0 は標準エラー
                    if (firstValue == InitialValue)
                    {
                        SetErrorState(ErrMsgUndefined);
                    }
                    else
                    {
                        SetErrorState(ErrMsgDivByZero);
                    }
                    return;
                }

                decimal result = Calculate(firstValue, currentValue, mType);
                firstValue = result;
            }
        }

        /// <summary>
        /// 途中式表示を「A op」形式に更新する（末尾に＝は付けない）。
        /// </summary>
        private void UpdateExpressionDisplay(decimal value, OperatorType type)
        {
            string op = GetOperatorSymbol(type);
            textExpression.Text = string.Format("{0} {1}", FormatNumberForExpression(value), op);
        }

        /// <summary>
        /// ＝のロジックを実行する。初回/連続＝で左右の扱いを切り替え、途中式（A op B =）を確定して結果を返す。
        /// </summary>
        private decimal ProcessEqualsLogic()
        {
            decimal currentValue = GetCurrentValue();
            bool isFirstEqual = !ExpressionEndsWithEqual();

            if (mType == OperatorType.NON)
            {
                secondValue = currentValue;
                textExpression.Text = string.Format("{0} {1}", FormatNumberForExpression(currentValue), EqualSymbol);
                // ＝単独でも結果を基数として保持する
                firstValue = currentValue;
                return currentValue;
            }

            decimal left = isFirstEqual ? firstValue : currentValue;
            decimal right = isFirstEqual ? currentValue : secondValue;

            if (mType == OperatorType.DIVIDE && right == InitialValue)
            {
                if (left == InitialValue)
                {
                    throw new InvalidOperationException(ErrMsgUndefined);
                }
                else
                {
                    throw new InvalidOperationException(ErrMsgDivByZero);
                }
            }

            decimal result = Calculate(left, right, mType);

            // ＝確定時に結果を新しい基数 A として保持する（% の基数も Windows 準拠とする）
            firstValue = result;

            textExpression.Text = string.Format("{0} {1} {2} {3}",
                FormatNumberForExpression(left),
                GetOperatorSymbol(mType),
                FormatNumberForExpression(right),
                EqualSymbol);

            return result;
        }

        /// <summary>
        /// 現在値を％（/100）へ変換する。
        /// </summary>
        private decimal CalculatePercent(decimal value)
        {
            return value * PercentMultiplier;
        }

        /// <summary>
        /// ％の表示と式を更新する。加減算は A*(B/100)、乗除算は B/100 として扱う。
        /// 表示は編集継続可能な状態（上書きオフ）にする。
        /// </summary>
        private void UpdatePercentDisplay(decimal percentValue)
        {
            decimal previousValue = firstValue;
            decimal calculatedValue;

            if (mType == OperatorType.ADD || mType == OperatorType.SUBTRACT)
            {
                calculatedValue = previousValue * percentValue; // A * (B/100)
                textExpression.Text = string.Format("{0} {1} {2}",
                    FormatNumberForExpression(previousValue),
                    GetOperatorSymbol(mType),
                    FormatNumberForExpression(calculatedValue));
            }
            else
            {
                calculatedValue = percentValue; // B% = B/100
                textExpression.Text = string.Format("{0} {1} {2}",
                    FormatNumberForExpression(previousValue),
                    GetOperatorSymbol(mType),
                    FormatNumberForExpression(calculatedValue));
            }

            // % 後は編集できるよう overwrite=false（BS 可）
            DisplayNumber(calculatedValue, false);
        }

        // ================= Backspace / ± =================

        /// <summary>
        /// 結果表示の末尾 1 文字を削除する。空や単独の「-」になった場合は 0 に戻す。
        /// </summary>
        private void HandleBackspace()
        {
            string currentText = textResult.Text.Replace(",", "");
            if (currentText.Length > 0)
            {
                string newText = currentText.Substring(0, currentText.Length - 1);

                if (string.IsNullOrEmpty(newText) || newText == "-")
                {
                    textResult.Text = ZeroValue;
                    text_overwrite = true;
                    Num_Dot = false;
                }
                else
                {
                    textResult.Text = newText;
                    Num_Dot = textResult.Text.Contains(".");
                }
            }
            else
            {
                ResetCalculatorState();
            }
        }

        /// <summary>
        /// 数値として符号反転を行い、統一出口経由で表示を更新する。
        /// </summary>
        private void ToggleSign()
        {
            displayValue = GetCurrentValue();
            displayValue = -displayValue;
            DisplayNumber(displayValue, false);
        }

        /// <summary>
        /// カンマ無しの生文字列の符号のみを反転する（末尾ゼロなどの見た目を保持する）。
        /// </summary>
        private string ToggleSignRaw(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return raw;
            }
            if (raw[0] == '-')
            {
                return raw.Substring(1);
            }
            return "-" + raw;
        }

        /// <summary>
        /// 生文字列をそのまま結果欄に反映し、カンマ付与を行う。
        /// </summary>
        private void SetTextFromRawPreservingCommas(string raw)
        {
            textResult.Text = raw;
            UpdateTextResultWithCommas();
        }

        /// <summary>
        /// ＝直後の ± 押下時に、式を negate(...) の入れ子表記で更新する。
        /// 例）"100 =" → "negate(100) =" → さらに ± → "negate(negate(100)) ="
        /// </summary>
        private void UpdateExpressionForToggleSign()
        {
            if (!ExpressionEndsWithEqual())
            {
                return;
            }

            string expr = textExpression.Text;
            int eq = expr.LastIndexOf(EqualSymbol);
            string body = (eq >= 0 ? expr.Substring(0, eq) : expr).Trim();

            // もし式本体が空なら firstValue を使う（＝直後は firstValue が最新結果）
            if (string.IsNullOrEmpty(body))
            {
                body = FormatNumberForExpression(firstValue);
            }

            string newBody = string.Format("{0}({1})", NegateFuncName, body);
            textExpression.Text = newBody + " " + EqualSymbol;
        }

        // ================= 表示の統一出口／整形 =================

        /// <summary>
        /// 表示の統一出口である。丸め → 表示規則に基づく文字列化 → カンマ付与 → 状態更新を行う。
        /// </summary>
        private void DisplayNumber(decimal value, bool overwrite)
        {
            decimal rounded = RoundResult(value);
            textResult.Text = FormatNumberForDisplay(rounded);
            UpdateTextResultWithCommas();
            text_overwrite = overwrite;
            Num_Dot = false; // 上書きモードに入る時は必ず false

            // 計算由来の表示なので、±の見た目保持はオフ
            preserveFormatOnToggle = false;
            lastUserTypedRaw = textResult.Text.Replace(",", "");
        }

        /// <summary>
        /// 結果表示の文字列に 3 桁区切りカンマを付与する（指数表記時はスキップする）。
        /// </summary>
        private void UpdateTextResultWithCommas()
        {
            if (IsError())
            {
                return;
            }
            if (IsExponentDisplay())
            {
                return;
            }

            string currentText = textResult.Text.Replace(",", "");
            if (string.IsNullOrEmpty(currentText) || currentText == "-" || (currentText == "0" && !Num_Dot))
            {
                return;
            }

            bool isNegative = currentText.StartsWith("-");
            if (isNegative)
            {
                currentText = currentText.Substring(1);
            }

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

                    if (dotIndex != -1)
                    {
                        newText += "." + decimalPart;
                    }
                    if (isNegative)
                    {
                        newText = "-" + newText;
                    }

                    textResult.Text = newText;
                }
            }
            catch (FormatException)
            {
                // 何もしない
            }
        }

        /// <summary>
        /// 途中式用の数値整形（結果表示と同一規則）を行う。
        /// </summary>
        private string FormatNumberForExpression(decimal value)
        {
            return FormatNumberForDisplay(value);
        }

        /// <summary>
        /// 指数表記の整形を行う。指数は小文字 e、不要な 0/小数点を整理する。
        /// </summary>
        private string FormatExponential(decimal value)
        {
            string gFormat = value.ToString("G15", CultureInfo.InvariantCulture);
            string expString = gFormat.Contains("E")
                ? gFormat
                : decimal.Parse(gFormat, CultureInfo.InvariantCulture).ToString("E", CultureInfo.InvariantCulture);

            expString = expString.Replace("E+", "e+").Replace("E-", "e-");
            string[] parts = expString.Split(new char[] { 'e' });

            string mantissa = parts[0].TrimEnd('0');
            if (mantissa.EndsWith("."))
            {
                mantissa = mantissa.TrimEnd('.');
            }
            if (!mantissa.Contains("."))
            {
                mantissa += ".";
            }

            string exponent = Regex.Replace(parts[1], @"^(\+|-)(0)(\d+)", "$1$3");
            return mantissa + "e" + exponent;
        }

        /// <summary>
        /// 表示規則である：
        /// ・abs &gt;= 1 ：整数部桁数 &lt;= 16 なら十進、超えたら指数
        /// ・0 &lt; abs &lt; 1：小数点以下桁数 &lt;= 17 なら十進、超えたら指数
        /// </summary>
        private string FormatNumberForDisplay(decimal value)
        {
            decimal abs = Math.Abs(value);
            if (abs == 0m)
            {
                return ZeroValue;
            }

            // 十進固定フォーマット（指数にならない／末尾0は落ちる）
            string fixedStr = value.ToString("0.#############################", CultureInfo.InvariantCulture);

            if (abs >= 1m)
            {
                // 整数部長（符号除外）
                int dot = fixedStr.IndexOf('.');
                bool neg = (fixedStr[0] == '-');
                int intLen = (dot >= 0 ? dot : fixedStr.Length) - (neg ? 1 : 0);

                if (intLen > DisplayMaxIntegerDigits)
                {
                    return FormatExponential(value);
                }
                return fixedStr;
            }
            else
            {
                // 0.～ の小数：小数部桁数で判定
                int dot = fixedStr.IndexOf('.');
                int fracLen = (dot >= 0) ? (fixedStr.Length - dot - 1) : 0;

                if (fracLen > DisplayMaxFractionDigitsLeadingZero)
                {
                    return FormatExponential(value);
                }
                return fixedStr;
            }
        }

        /// <summary>
        /// 現在の表示文字列を decimal に変換する（指数は double を経由する）。
        /// </summary>
        private decimal GetCurrentValue()
        {
            return ParseDisplayToDecimal(textResult.Text);
        }

        /// <summary>
        /// 表示文字列を decimal にパースする。指数表記は double 経由で変換する。
        /// </summary>
        private decimal ParseDisplayToDecimal(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return InitialValue;
            }

            string s = text.Replace(",", "");
            decimal dv;

            if (s.IndexOf('e') >= 0 || s.IndexOf('E') >= 0)
            {
                double dd;
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out dd))
                {
                    try
                    {
                        return (decimal)dd;
                    }
                    catch
                    {
                        return InitialValue;
                    }
                }
                return InitialValue;
            }

            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out dv))
            {
                return dv;
            }
            return InitialValue;
        }

        /// <summary>
        /// 桁数に基づく丸めを行う。abs&lt;1 は小数17桁、abs≥1 は「有効桁16」を保つよう丸める。
        /// 指数域（切替後）は丸めない。
        /// </summary>
        private decimal RoundResult(decimal value)
        {
            decimal abs = Math.Abs(value);

            // 0 < abs < 1 は 17 桁まで十進表示を許容
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

        // ================= 状況判定 =================

        /// <summary>
        /// エラー状態かどうかを返す。
        /// </summary>
        private bool IsError()
        {
            return isErrorState;
        }

        /// <summary>
        /// 式表示が ＝ で終わっているか判定する（＝直後の特別挙動に使用する）。
        /// </summary>
        private bool ExpressionEndsWithEqual()
        {
            return textExpression.Text.Length > 0 && textExpression.Text.EndsWith(EqualSymbol);
        }

        /// <summary>
        /// 現在の結果表示が指数表記か判定する。
        /// </summary>
        private bool IsExponentDisplay()
        {
            string t = textResult.Text;
            return (t.IndexOf('e') >= 0 || t.IndexOf('E') >= 0);
        }

        /// <summary>
        /// エラー時に自動リセットを行い、呼び出し元に続行可否を返す。
        /// </summary>
        private bool ShouldResetOnError()
        {
            if (isErrorState)
            {
                ResetCalculatorState();
                return true;
            }
            return false;
        }

        // ================= 式欄ユーティリティ =================

        /// <summary>
        /// 式表示を常に末尾（右端）までスクロールさせる。
        /// </summary>
        private void AutoScrollExpressionToEnd()
        {
            int len = textExpression.Text.Length;
            textExpression.SelectionStart = (len > 0) ? len : 0;
            textExpression.SelectionLength = 0;
        }

        // ================= フォント／レイアウト =================

        /// <summary>
        /// 候補フォントのうち最初に生成できたものを返す。なければフォールバックのファミリを使用する。
        /// </summary>
        private Font CreateFirstAvailableFont(string[] families, float size, FontStyle style, Font fallback)
        {
            foreach (string fam in families)
            {
                try
                {
                    using (Font probe = new Font(fam, size, style))
                    {
                        return new Font(fam, size, style);
                    }
                }
                catch
                {
                    // 作れないフォント名はスキップ
                }
            }
            return new Font(fallback.FontFamily, size, style);
        }

        /// <summary>
        /// 結果欄フォントを基準サイズから“縮小のみ”で枠内に収める（Windows 電卓風）。拡大はしない。
        /// </summary>
        private void AutoFitResultFontWindowsLike()
        {
            float size = defaultFontSize; // 基準サイズ

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

        // ================= エラー／状態リセット =================

        /// <summary>
        /// エラー時に一部ボタンの活性/非活性をまとめて切り替える。
        /// </summary>
        private void SetButtonsEnabled(bool enabled)
        {
            foreach (Button btn in DisabledButtonsOnError)
            {
                btn.Enabled = enabled;
            }
        }

        /// <summary>
        /// エラー状態を設定し、メッセージ表示・フォントサイズ調整・ボタン無効化を行う。
        /// </summary>
        private void SetErrorState(string message)
        {
            textResult.Text = message;

            float sz = ErrorFontSize;
            if (sz < WinMinFontSize)
            {
                sz = WinMinFontSize;
            }
            textResult.Font = new Font(textResult.Font.FontFamily, sz, textResult.Font.Style);

            isErrorState = true;
            SetButtonsEnabled(false);
        }

        /// <summary>
        /// 入力開始前の状態を整える。エラー中や ＝直後の場合は状態を初期化する。
        /// </summary>
        private void HandleInitialState()
        {
            if (isErrorState || ExpressionEndsWithEqual())
            {
                ResetCalculatorState();
                clearedExprAfterEqual = false;
            }
        }

        /// <summary>
        /// 全体のリセット処理である。計算状態と UI 状態を初期化し、ボタンを活性化する。
        /// </summary>
        private void ResetAllState()
        {
            ResetCalculatorState();
            SetButtonsEnabled(true);
        }

        /// <summary>
        /// 計算機内部状態と表示、フォントの初期化を行う（結果欄は基準サイズに戻し、再フィットも実施する）。
        /// </summary>
        private void ResetCalculatorState()
        {
            firstValue = InitialValue;
            secondValue = InitialValue;
            mType = OperatorType.NON;
            textExpression.Text = "";
            textResult.Text = ZeroValue;
            text_overwrite = true;
            Num_Dot = false;
            isErrorState = false;
            isClearEntry = false;
            displayValue = InitialValue;

            preserveFormatOnToggle = false;
            lastUserTypedRaw = ZeroValue;
            lastActionWasPercent = false;
            clearedExprAfterEqual = false;

            // フォントを基準サイズに戻す
            float rs = (defaultFontSize > 0) ? defaultFontSize : WinResultBaseSize;
            textResult.Font = new Font(textResult.Font.FontFamily, rs, textResult.Font.Style);

            float es = (defaultExpressionFontSize > 0) ? defaultExpressionFontSize : WinExprBaseSize;
            textExpression.Font = new Font(textExpression.Font.FontFamily, es, textExpression.Font.Style);

            // 結果欄のみ自動フィット（式欄は固定）
            AutoFitResultFontWindowsLike();
        }
    }
}
