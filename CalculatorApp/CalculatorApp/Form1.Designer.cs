﻿namespace CalculatorApp
{
	partial class Form1
	{
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
            this.button1 = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnClearEntry = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnTogglesign = new System.Windows.Forms.Button();
            this.btnNum7 = new System.Windows.Forms.Button();
            this.btnPercent = new System.Windows.Forms.Button();
            this.btnNum8 = new System.Windows.Forms.Button();
            this.btnNum9 = new System.Windows.Forms.Button();
            this.btnDivide = new System.Windows.Forms.Button();
            this.btnNum4 = new System.Windows.Forms.Button();
            this.btnNum5 = new System.Windows.Forms.Button();
            this.btnNum6 = new System.Windows.Forms.Button();
            this.btnMultiply = new System.Windows.Forms.Button();
            this.btnNum1 = new System.Windows.Forms.Button();
            this.btnNum2 = new System.Windows.Forms.Button();
            this.btnMinus = new System.Windows.Forms.Button();
            this.btnNum3 = new System.Windows.Forms.Button();
            this.btnNum0 = new System.Windows.Forms.Button();
            this.btnDot = new System.Windows.Forms.Button();
            this.btnPlus = new System.Windows.Forms.Button();
            this.btnEnter = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnTopMost = new System.Windows.Forms.Button();
            this.textResult = new System.Windows.Forms.TextBox();
            this.textExpression = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(4, 561);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(0, 0);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            this.btnClear.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.Location = new System.Drawing.Point(206, 4);
            this.btnClear.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(91, 62);
            this.btnClear.TabIndex = 6;
            this.btnClear.Text = "C";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnClearEntry
            // 
            this.btnClearEntry.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClearEntry.Location = new System.Drawing.Point(105, 4);
            this.btnClearEntry.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnClearEntry.Name = "btnClearEntry";
            this.btnClearEntry.Size = new System.Drawing.Size(91, 62);
            this.btnClearEntry.TabIndex = 5;
            this.btnClearEntry.Text = "CE";
            this.btnClearEntry.UseVisualStyleBackColor = true;
            this.btnClearEntry.Click += new System.EventHandler(this.btnClearEntry_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.btnTogglesign, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.btnClearEntry, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnNum7, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnPercent, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnNum8, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnNum9, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnDivide, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnNum4, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnNum5, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnNum6, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnMultiply, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnNum1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnNum2, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnMinus, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnNum3, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnNum0, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.btnDot, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.btnPlus, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.btnEnter, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.btnBack, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnClear, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 227);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(405, 412);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // btnTogglesign
            // 
            this.btnTogglesign.Location = new System.Drawing.Point(4, 284);
            this.btnTogglesign.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnTogglesign.Name = "btnTogglesign";
            this.btnTogglesign.Size = new System.Drawing.Size(91, 62);
            this.btnTogglesign.TabIndex = 20;
            this.btnTogglesign.Text = "+/-";
            this.btnTogglesign.UseVisualStyleBackColor = true;
            this.btnTogglesign.Click += new System.EventHandler(this.btnTogglesign_Click);
            // 
            // btnNum7
            // 
            this.btnNum7.Location = new System.Drawing.Point(4, 74);
            this.btnNum7.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnNum7.Name = "btnNum7";
            this.btnNum7.Size = new System.Drawing.Size(91, 62);
            this.btnNum7.TabIndex = 8;
            this.btnNum7.Text = "7";
            this.btnNum7.UseVisualStyleBackColor = true;
            this.btnNum7.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btnPercent
            // 
            this.btnPercent.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPercent.Location = new System.Drawing.Point(4, 4);
            this.btnPercent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnPercent.Name = "btnPercent";
            this.btnPercent.Size = new System.Drawing.Size(91, 62);
            this.btnPercent.TabIndex = 4;
            this.btnPercent.Text = "%";
            this.btnPercent.UseVisualStyleBackColor = true;
            this.btnPercent.Click += new System.EventHandler(this.btnPercent_Click);
            // 
            // btnNum8
            // 
            this.btnNum8.Location = new System.Drawing.Point(105, 74);
            this.btnNum8.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnNum8.Name = "btnNum8";
            this.btnNum8.Size = new System.Drawing.Size(91, 62);
            this.btnNum8.TabIndex = 9;
            this.btnNum8.Text = "8";
            this.btnNum8.UseVisualStyleBackColor = true;
            this.btnNum8.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btnNum9
            // 
            this.btnNum9.Location = new System.Drawing.Point(206, 74);
            this.btnNum9.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnNum9.Name = "btnNum9";
            this.btnNum9.Size = new System.Drawing.Size(91, 62);
            this.btnNum9.TabIndex = 10;
            this.btnNum9.Text = "9";
            this.btnNum9.UseVisualStyleBackColor = true;
            this.btnNum9.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btnDivide
            // 
            this.btnDivide.Location = new System.Drawing.Point(307, 74);
            this.btnDivide.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnDivide.Name = "btnDivide";
            this.btnDivide.Size = new System.Drawing.Size(91, 62);
            this.btnDivide.TabIndex = 11;
            this.btnDivide.Text = "÷";
            this.btnDivide.UseVisualStyleBackColor = true;
            this.btnDivide.Click += new System.EventHandler(this.btnOperation_Click);
            // 
            // btnNum4
            // 
            this.btnNum4.Location = new System.Drawing.Point(4, 144);
            this.btnNum4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnNum4.Name = "btnNum4";
            this.btnNum4.Size = new System.Drawing.Size(91, 62);
            this.btnNum4.TabIndex = 12;
            this.btnNum4.Text = "4";
            this.btnNum4.UseVisualStyleBackColor = true;
            this.btnNum4.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btnNum5
            // 
            this.btnNum5.Location = new System.Drawing.Point(105, 144);
            this.btnNum5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnNum5.Name = "btnNum5";
            this.btnNum5.Size = new System.Drawing.Size(91, 62);
            this.btnNum5.TabIndex = 13;
            this.btnNum5.Text = "5";
            this.btnNum5.UseVisualStyleBackColor = true;
            this.btnNum5.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btnNum6
            // 
            this.btnNum6.Location = new System.Drawing.Point(206, 144);
            this.btnNum6.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnNum6.Name = "btnNum6";
            this.btnNum6.Size = new System.Drawing.Size(91, 62);
            this.btnNum6.TabIndex = 14;
            this.btnNum6.Text = "6";
            this.btnNum6.UseVisualStyleBackColor = true;
            this.btnNum6.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btnMultiply
            // 
            this.btnMultiply.Location = new System.Drawing.Point(307, 144);
            this.btnMultiply.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnMultiply.Name = "btnMultiply";
            this.btnMultiply.Size = new System.Drawing.Size(91, 62);
            this.btnMultiply.TabIndex = 15;
            this.btnMultiply.Text = "×";
            this.btnMultiply.UseVisualStyleBackColor = true;
            this.btnMultiply.Click += new System.EventHandler(this.btnOperation_Click);
            // 
            // btnNum1
            // 
            this.btnNum1.Location = new System.Drawing.Point(4, 214);
            this.btnNum1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnNum1.Name = "btnNum1";
            this.btnNum1.Size = new System.Drawing.Size(91, 62);
            this.btnNum1.TabIndex = 16;
            this.btnNum1.Text = "1";
            this.btnNum1.UseVisualStyleBackColor = true;
            this.btnNum1.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btnNum2
            // 
            this.btnNum2.Location = new System.Drawing.Point(105, 214);
            this.btnNum2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnNum2.Name = "btnNum2";
            this.btnNum2.Size = new System.Drawing.Size(91, 62);
            this.btnNum2.TabIndex = 17;
            this.btnNum2.Text = "2";
            this.btnNum2.UseVisualStyleBackColor = true;
            this.btnNum2.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btnMinus
            // 
            this.btnMinus.Location = new System.Drawing.Point(307, 214);
            this.btnMinus.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnMinus.Name = "btnMinus";
            this.btnMinus.Size = new System.Drawing.Size(91, 62);
            this.btnMinus.TabIndex = 19;
            this.btnMinus.Text = "-";
            this.btnMinus.UseVisualStyleBackColor = true;
            this.btnMinus.Click += new System.EventHandler(this.btnOperation_Click);
            // 
            // btnNum3
            // 
            this.btnNum3.Location = new System.Drawing.Point(206, 214);
            this.btnNum3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnNum3.Name = "btnNum3";
            this.btnNum3.Size = new System.Drawing.Size(91, 62);
            this.btnNum3.TabIndex = 18;
            this.btnNum3.Text = "3";
            this.btnNum3.UseVisualStyleBackColor = true;
            this.btnNum3.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btnNum0
            // 
            this.btnNum0.Location = new System.Drawing.Point(105, 284);
            this.btnNum0.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnNum0.Name = "btnNum0";
            this.btnNum0.Size = new System.Drawing.Size(91, 62);
            this.btnNum0.TabIndex = 21;
            this.btnNum0.Text = "0";
            this.btnNum0.UseVisualStyleBackColor = true;
            this.btnNum0.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btnDot
            // 
            this.btnDot.Location = new System.Drawing.Point(206, 284);
            this.btnDot.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnDot.Name = "btnDot";
            this.btnDot.Size = new System.Drawing.Size(91, 62);
            this.btnDot.TabIndex = 22;
            this.btnDot.Text = ".";
            this.btnDot.UseVisualStyleBackColor = true;
            this.btnDot.Click += new System.EventHandler(this.btnDot_Click);
            // 
            // btnPlus
            // 
            this.btnPlus.Location = new System.Drawing.Point(307, 284);
            this.btnPlus.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnPlus.Name = "btnPlus";
            this.btnPlus.Size = new System.Drawing.Size(91, 62);
            this.btnPlus.TabIndex = 23;
            this.btnPlus.Text = "+";
            this.btnPlus.UseVisualStyleBackColor = true;
            this.btnPlus.Click += new System.EventHandler(this.btnOperation_Click);
            // 
            // btnEnter
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.btnEnter, 5);
            this.btnEnter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnEnter.Location = new System.Drawing.Point(4, 354);
            this.btnEnter.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnEnter.Name = "btnEnter";
            this.btnEnter.Size = new System.Drawing.Size(397, 55);
            this.btnEnter.TabIndex = 24;
            this.btnEnter.Text = "=";
            this.btnEnter.UseVisualStyleBackColor = true;
            this.btnEnter.Click += new System.EventHandler(this.btnEnter_Click);
            // 
            // btnBack
            // 
            this.btnBack.Location = new System.Drawing.Point(307, 4);
            this.btnBack.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(91, 62);
            this.btnBack.TabIndex = 7;
            this.btnBack.Text = "←";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // btnTopMost
            // 
            this.btnTopMost.Location = new System.Drawing.Point(16, 15);
            this.btnTopMost.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnTopMost.Name = "btnTopMost";
            this.btnTopMost.Size = new System.Drawing.Size(40, 38);
            this.btnTopMost.TabIndex = 1;
            this.btnTopMost.Text = "○";
            this.btnTopMost.UseVisualStyleBackColor = true;
            this.btnTopMost.Click += new System.EventHandler(this.btnTopMost_Click);
            // 
            // textResult
            // 
            this.textResult.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textResult.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Bold);
            this.textResult.Location = new System.Drawing.Point(5, 134);
            this.textResult.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textResult.Name = "textResult";
            this.textResult.ReadOnly = true;
            this.textResult.Size = new System.Drawing.Size(387, 80);
            this.textResult.TabIndex = 3;
            this.textResult.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textResult.TextChanged += new System.EventHandler(this.textResult_TextChanged);
            // 
            // textExpression
            // 
            this.textExpression.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textExpression.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.textExpression.Location = new System.Drawing.Point(5, 95);
            this.textExpression.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textExpression.Name = "textExpression";
            this.textExpression.ReadOnly = true;
            this.textExpression.Size = new System.Drawing.Size(387, 23);
            this.textExpression.TabIndex = 2;
            this.textExpression.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textExpression.TextChanged += new System.EventHandler(this.textExpression_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(405, 639);
            this.Controls.Add(this.textExpression);
            this.Controls.Add(this.textResult);
            this.Controls.Add(this.btnTopMost);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.button1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.Button btnClearEntry;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button btnBack;
		private System.Windows.Forms.Button btnNum7;
		private System.Windows.Forms.Button btnTogglesign;
		private System.Windows.Forms.Button btnPercent;
		private System.Windows.Forms.Button btnNum8;
		private System.Windows.Forms.Button btnNum9;
		private System.Windows.Forms.Button btnDivide;
		private System.Windows.Forms.Button btnNum4;
		private System.Windows.Forms.Button btnNum5;
		private System.Windows.Forms.Button btnNum6;
		private System.Windows.Forms.Button btnMultiply;
		private System.Windows.Forms.Button btnNum1;
		private System.Windows.Forms.Button btnNum2;
		private System.Windows.Forms.Button btnMinus;
		private System.Windows.Forms.Button btnNum3;
		private System.Windows.Forms.Button btnNum0;
		private System.Windows.Forms.Button btnDot;
		private System.Windows.Forms.Button btnPlus;
		private System.Windows.Forms.Button btnEnter;
		private System.Windows.Forms.Button btnTopMost;
		private System.Windows.Forms.TextBox textResult;
		private System.Windows.Forms.TextBox textExpression;
	}
}

