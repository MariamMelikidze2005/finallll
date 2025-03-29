using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace forfinalofC_
{
    public partial class Form1 : Form
    {
        private readonly string connectionString = "Server=DESKTOP-JFOLDL4\\SQLEXPRESS;Database=LibraryDB;Integrated Security=True;";

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await LoadAuthors();
            await LoadBooks();
        }

        private async Task LoadAuthors()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("SELECT DISTINCT AuthorId, AuthorName FROM Authors", conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    DataTable dt = new DataTable();
                    dt.Load(reader);

                    foreach (DataColumn col in dt.Columns)
                    {
                        Console.WriteLine(col.ColumnName);
                    }

                    if (dt.Columns.Contains("Name"))
                    {
                        comboAuthors.DataSource = dt;
                        comboAuthors.DisplayMember = "Name";  
                        comboAuthors.ValueMember = "AuthorId";
                        comboAuthors.SelectedIndex = -1;
                    }
                    else
                    {
                        MessageBox.Show("AuthorName column not found in database results.");
                    }
                }
            }
        }

        private async Task LoadBooks()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Books", conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    dataGridView1.DataSource = dt;
                    comboBooks.DataSource = dt;
                    comboBooks.DisplayMember = "Title";
                    comboBooks.ValueMember = "BookId";
                    comboBooks.SelectedIndex = -1;
                }
            }
        }

        private async void btnLoadBooks_Click(object sender, EventArgs e)
        {
            await LoadBooks();
        }

        private async void btnInsertBook_Click(object sender, EventArgs e)
        {
            if (comboAuthors.SelectedValue == null || string.IsNullOrWhiteSpace(txtTitle.Text) ||
                string.IsNullOrWhiteSpace(txtPublishedYear.Text) || string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("Please fill in all fields and select an author.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("InsertBook", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Title", txtTitle.Text);
                    cmd.Parameters.AddWithValue("@AuthorId", comboAuthors.SelectedValue);
                    cmd.Parameters.AddWithValue("@PublishedYear", Convert.ToInt32(txtPublishedYear.Text));
                    cmd.Parameters.AddWithValue("@Price", Convert.ToDecimal(txtPrice.Text));

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            MessageBox.Show("Book Inserted Successfully!");
            await LoadBooks();
        }

        private async void btnUpdateBook_Click(object sender, EventArgs e)
        {
            if (comboBooks.SelectedValue == null)
            {
                MessageBox.Show("Please select a book to update.");
                return;
            }

            if (comboAuthors.SelectedValue == null)
            {
                MessageBox.Show("Please select an author.");
                return;
            }

            int authorId;
            if (!int.TryParse(comboAuthors.SelectedValue.ToString(), out authorId))
            {
                MessageBox.Show("Invalid Author ID.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("UpdateBook", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BookId", comboBooks.SelectedValue);
                    cmd.Parameters.AddWithValue("@Title", txtTitle.Text);
                    cmd.Parameters.AddWithValue("@AuthorId", authorId); 
                    cmd.Parameters.AddWithValue("@PublishedYear", Convert.ToInt32(txtPublishedYear.Text));
                    cmd.Parameters.AddWithValue("@Price", Convert.ToDecimal(txtPrice.Text));

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            MessageBox.Show("Book Updated Successfully!");
            await LoadBooks();
        }


        private async void btnDeleteBook_Click(object sender, EventArgs e)
        {
            if (comboBooks.SelectedValue == null)
            {
                MessageBox.Show("Please select a book to delete.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("DeleteBook", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BookId", comboBooks.SelectedValue);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            MessageBox.Show("Book Deleted Successfully!");
            await LoadBooks();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var row = dataGridView1.SelectedRows[0];

                txtTitle.Text = row.Cells["Title"].Value.ToString();
                comboAuthors.SelectedValue = row.Cells["AuthorId"].Value; 
                txtPublishedYear.Text = row.Cells["PublishedYear"].Value.ToString();
                txtPrice.Text = row.Cells["Price"].Value.ToString();
            }
        }

        private void comboAuthors_SelectedIndexChanged(object sender, EventArgs e)
        {
            // You can implement filtering books based on author selection here if needed.
        }

        private void txtTitle_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
