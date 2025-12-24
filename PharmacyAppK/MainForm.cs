using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace PharmacyApp
{
    public partial class MainForm : Form
    {
        private DatabaseContext db = new DatabaseContext();
        private DataTable cartTable;
        private int currentUserId;
        private string currentUserRole;

        public MainForm(int userId, string role)
        {
            InitializeComponent();
            
            dgvMedicines.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMedicines.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvMedicines.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvMedicines.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMedicines.MultiSelect = false;
            dgvMedicines.ReadOnly = true;

            dgvCart.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCart.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvCart.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvCart.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCart.MultiSelect = false;
            dgvCart.AllowUserToAddRows = false;
            dgvCart.ReadOnly = true;

            currentUserId = userId;
            currentUserRole = role;

            if (currentUserRole != "Admin")
            {
                btnAdminPanel.Visible = false;
            }

            InitializeCart();
            LoadMedicines();

            dgvMedicines.CellDoubleClick += dgvMedicines_CellDoubleClick;
            dgvMedicines.CellFormatting += dgvMedicines_CellFormatting;
        }

        private void InitializeCart()
        {
            cartTable = new DataTable();
            cartTable.Columns.Add("StockID", typeof(int));
            cartTable.Columns.Add("Name", typeof(string));
            cartTable.Columns.Add("Price", typeof(decimal));
            cartTable.Columns.Add("Quantity", typeof(int));
            cartTable.Columns.Add("Sum", typeof(decimal), "Price * Quantity");
            dgvCart.DataSource = cartTable;
        }

        private void LoadMedicines(string search = "")
        {
            string query = @"
                SELECT s.StockID, m.Name, m.Manufacturer, s.SellingPrice as [Ціна], 
                s.Quantity as [Кількість], s.ExpiryDate as [Термін]
                FROM Stock s
                JOIN Medicines m ON s.MedicineID = m.MedicineID
                WHERE s.Quantity > 0";

            SqlParameter[] parameters = null;

            if (!string.IsNullOrEmpty(search))
            {
                query += " AND m.Name LIKE @search";
                parameters = new SqlParameter[] 
                { 
                    new SqlParameter("@search", "%" + search + "%") 
                };
            }

            try
            {
                dgvMedicines.DataSource = db.ExecuteQuery(query, parameters);

                if (dgvMedicines.Columns["Manufacturer"] != null)
                    dgvMedicines.Columns["Manufacturer"].FillWeight = 150;
                
                if (dgvMedicines.Columns["Термін"] != null)
                    dgvMedicines.Columns["Термін"].FillWeight = 120;
                
                if (dgvMedicines.Columns["Кількість"] != null)
                    dgvMedicines.Columns["Кількість"].FillWeight = 80;
            }
            catch
            {
                MessageBox.Show("Критична помилка БД", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void dgvMedicines_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvMedicines.Columns[e.ColumnIndex].Name == "Термін" && e.Value != null)
            {
                DateTime expiryDate = Convert.ToDateTime(e.Value);
                
                if (expiryDate <= DateTime.Now.AddDays(30))
                {
                    dgvMedicines.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                }
                else
                {
                    dgvMedicines.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadMedicines(txtSearch.Text);
        }

        private void btnAddToCart_Click(object sender, EventArgs e)
        {
             AddToCart();
        }

        private void dgvMedicines_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            AddToCart();
        }

        private void AddToCart()
        {
            if (dgvMedicines.CurrentRow != null)
            {
                DateTime expiryDate = Convert.ToDateTime(dgvMedicines.CurrentRow.Cells["Термін"].Value);
                if (expiryDate < DateTime.Now)
                {
                    MessageBox.Show("Термін придатності вийшов!", "Блокування", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int stockId = (int)dgvMedicines.CurrentRow.Cells["StockID"].Value;
                string name = dgvMedicines.CurrentRow.Cells["Name"].Value.ToString();
                decimal price = (decimal)dgvMedicines.CurrentRow.Cells["Ціна"].Value;
                int maxQty = (int)dgvMedicines.CurrentRow.Cells["Кількість"].Value;

                string input = Microsoft.VisualBasic.Interaction.InputBox($"Скільки пачок '{name}' додати?", "Кількість", "1");
                if (int.TryParse(input, out int qty) && qty > 0)
                {
                    if (qty > maxQty)
                    {
                        MessageBox.Show("Недостатньо товару", "Склад", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                    cartTable.Rows.Add(stockId, name, price, qty);
                    UpdateTotal();
                }
            }
        }

        private void btnDeleteFromCart_Click(object sender, EventArgs e)
        {
            if (dgvCart.CurrentRow != null)
            {
                dgvCart.Rows.Remove(dgvCart.CurrentRow);
                UpdateTotal();
            }
        }

        private void btnAdminPanel_Click(object sender, EventArgs e)
        {
            AdminForm adminForm = new AdminForm();
            adminForm.ShowDialog();
            LoadMedicines();
        }

        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (DataRow row in cartTable.Rows) total += (decimal)row["Sum"];
            lblTotal.Text = total.ToString("F2") + " грн";
        }

        private void btnPay_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0) return;

            try
            {
                decimal totalAmount = 0;
                foreach (DataRow row in cartTable.Rows)
                {
                    totalAmount += (decimal)row["Sum"];
                }

                string saleQuery = "INSERT INTO Sales (SaleDate, TotalAmount, UserID) VALUES (@date, @total, @user); SELECT SCOPE_IDENTITY();";
                
                SqlParameter[] saleParams = new SqlParameter[]
                {
                    new SqlParameter("@date", DateTime.Now),
                    new SqlParameter("@total", totalAmount),
                    new SqlParameter("@user", currentUserId)
                };

                int saleId = db.ExecuteScalar(saleQuery, saleParams);

                foreach (DataRow row in cartTable.Rows)
                {
                    int stockId = (int)row["StockID"];
                    int quantity = (int)row["Quantity"];
                    decimal price = (decimal)row["Price"];

                    int medicineId = 0;
                    string getMedIdQuery = $"SELECT MedicineID FROM Stock WHERE StockID = {stockId}";
                    medicineId = db.ExecuteScalar(getMedIdQuery);

                    string itemQuery = "INSERT INTO SaleItems (SaleID, MedicineID, Quantity, PriceAtSale) VALUES (@saleId, @medId, @qty, @price)";
                    SqlParameter[] itemParams = new SqlParameter[]
                    {
                        new SqlParameter("@saleId", saleId),
                        new SqlParameter("@medId", medicineId),
                        new SqlParameter("@qty", quantity),
                        new SqlParameter("@price", price)
                    };
                    db.ExecuteNonQuery(itemQuery, itemParams);

                    string stockQuery = "UPDATE Stock SET Quantity = Quantity - @qty WHERE StockID = @stockId";
                    SqlParameter[] stockParams = new SqlParameter[]
                    {
                        new SqlParameter("@qty", quantity),
                        new SqlParameter("@stockId", stockId)
                    };
                    db.ExecuteNonQuery(stockQuery, stockParams);
                }

                MessageBox.Show("Продаж успішно проведено", "Каса", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cartTable.Rows.Clear();
                UpdateTotal();
                LoadMedicines(txtSearch.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при проведенні продажу: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}