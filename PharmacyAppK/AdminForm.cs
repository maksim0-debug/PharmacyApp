using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace PharmacyApp
{
    public partial class AdminForm : Form
    {
        private DatabaseContext db = new DatabaseContext();
        private ComboBox cmbMedicines;
        private TextBox txtQuantity;
        private TextBox txtPurchasePrice;
        private TextBox txtSellingPrice;
        private DateTimePicker dtpExpiry;
        private Button btnAddStock;
        private Label lblMedicine;
        private Label lblQuantity;
        private Label lblPurchasePrice;
        private Label lblSellingPrice;
        private Label lblExpiry;

        public AdminForm()
        {
            InitializeComponent();
            LoadMedicinesCombo();
        }

        private void InitializeComponent()
        {
            this.cmbMedicines = new System.Windows.Forms.ComboBox();
            this.txtQuantity = new System.Windows.Forms.TextBox();
            this.txtPurchasePrice = new System.Windows.Forms.TextBox();
            this.txtSellingPrice = new System.Windows.Forms.TextBox();
            this.dtpExpiry = new System.Windows.Forms.DateTimePicker();
            this.btnAddStock = new System.Windows.Forms.Button();
            this.lblMedicine = new System.Windows.Forms.Label();
            this.lblQuantity = new System.Windows.Forms.Label();
            this.lblPurchasePrice = new System.Windows.Forms.Label();
            this.lblSellingPrice = new System.Windows.Forms.Label();
            this.lblExpiry = new System.Windows.Forms.Label();
            this.SuspendLayout();

            this.cmbMedicines.FormattingEnabled = true;
            this.cmbMedicines.Location = new System.Drawing.Point(120, 20);
            this.cmbMedicines.Name = "cmbMedicines";
            this.cmbMedicines.Size = new System.Drawing.Size(200, 21);
            this.cmbMedicines.TabIndex = 0;

            this.txtQuantity.Location = new System.Drawing.Point(120, 60);
            this.txtQuantity.Name = "txtQuantity";
            this.txtQuantity.Size = new System.Drawing.Size(100, 20);
            this.txtQuantity.TabIndex = 1;

            this.txtPurchasePrice.Location = new System.Drawing.Point(120, 100);
            this.txtPurchasePrice.Name = "txtPurchasePrice";
            this.txtPurchasePrice.Size = new System.Drawing.Size(100, 20);
            this.txtPurchasePrice.TabIndex = 2;

            this.txtSellingPrice.Location = new System.Drawing.Point(120, 140);
            this.txtSellingPrice.Name = "txtSellingPrice";
            this.txtSellingPrice.Size = new System.Drawing.Size(100, 20);
            this.txtSellingPrice.TabIndex = 3;

            this.dtpExpiry.Location = new System.Drawing.Point(120, 180);
            this.dtpExpiry.Name = "dtpExpiry";
            this.dtpExpiry.Size = new System.Drawing.Size(200, 20);
            this.dtpExpiry.TabIndex = 4;

            this.btnAddStock.Location = new System.Drawing.Point(120, 220);
            this.btnAddStock.Name = "btnAddStock";
            this.btnAddStock.Size = new System.Drawing.Size(120, 30);
            this.btnAddStock.TabIndex = 5;
            this.btnAddStock.Text = "Оприбуткувати";
            this.btnAddStock.UseVisualStyleBackColor = true;
            this.btnAddStock.Click += new System.EventHandler(this.btnAddStock_Click);

            this.lblMedicine.AutoSize = true;
            this.lblMedicine.Location = new System.Drawing.Point(20, 23);
            this.lblMedicine.Text = "Ліки:";
            
            this.lblQuantity.AutoSize = true;
            this.lblQuantity.Location = new System.Drawing.Point(20, 63);
            this.lblQuantity.Text = "Кількість:";

            this.lblPurchasePrice.AutoSize = true;
            this.lblPurchasePrice.Location = new System.Drawing.Point(20, 103);
            this.lblPurchasePrice.Text = "Закуп. ціна:";

            this.lblSellingPrice.AutoSize = true;
            this.lblSellingPrice.Location = new System.Drawing.Point(20, 143);
            this.lblSellingPrice.Text = "Ціна продажу:";

            this.lblExpiry.AutoSize = true;
            this.lblExpiry.Location = new System.Drawing.Point(20, 186);
            this.lblExpiry.Text = "Термін:";

            this.ClientSize = new System.Drawing.Size(350, 280);
            this.Controls.Add(this.lblExpiry);
            this.Controls.Add(this.lblSellingPrice);
            this.Controls.Add(this.lblPurchasePrice);
            this.Controls.Add(this.lblQuantity);
            this.Controls.Add(this.lblMedicine);
            this.Controls.Add(this.btnAddStock);
            this.Controls.Add(this.dtpExpiry);
            this.Controls.Add(this.txtSellingPrice);
            this.Controls.Add(this.txtPurchasePrice);
            this.Controls.Add(this.txtQuantity);
            this.Controls.Add(this.cmbMedicines);
            this.Name = "AdminForm";
            this.Text = "Прихідна накладна";
            this.StartPosition = FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadMedicinesCombo()
        {
            string query = "SELECT MedicineID, Name FROM Medicines";
            DataTable dt = db.ExecuteQuery(query);
            
            cmbMedicines.DataSource = dt;
            cmbMedicines.DisplayMember = "Name";
            cmbMedicines.ValueMember = "MedicineID";
        }

        private void btnAddStock_Click(object sender, EventArgs e)
        {
            if (cmbMedicines.SelectedValue == null || 
                string.IsNullOrWhiteSpace(txtQuantity.Text) || 
                string.IsNullOrWhiteSpace(txtPurchasePrice.Text) || 
                string.IsNullOrWhiteSpace(txtSellingPrice.Text))
            {
                MessageBox.Show("Заповніть всі поля!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int medicineId = Convert.ToInt32(cmbMedicines.SelectedValue);
                
                if (!int.TryParse(txtQuantity.Text, out int quantity))
                {
                    MessageBox.Show("Некоректна кількість!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string sysSep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                string pText = txtPurchasePrice.Text.Replace(".", sysSep).Replace(",", sysSep);
                string sText = txtSellingPrice.Text.Replace(".", sysSep).Replace(",", sysSep);

                if (!decimal.TryParse(pText, out decimal purchasePrice) || !decimal.TryParse(sText, out decimal sellingPrice))
                {
                    MessageBox.Show("Некоректний формат ціни!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string query = "INSERT INTO Stock (MedicineID, Quantity, PurchasePrice, SellingPrice, ExpiryDate) VALUES (@medId, @qty, @pPrice, @sPrice, @expiry)";
                
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@medId", medicineId),
                    new SqlParameter("@qty", quantity),
                    new SqlParameter("@pPrice", purchasePrice),
                    new SqlParameter("@sPrice", sellingPrice),
                    new SqlParameter("@expiry", dtpExpiry.Value)
                };

                db.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Товар успішно додано на склад!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при додаванні: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
