using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace XMLProcessor
{
    public class NodoForm : Form
    {
        // Controles
        private Label lblNombre;
        private TextBox txtNombre;
        private Label lblValor;
        private TextBox txtValor;
        private Label lblAtributos;
        private DataGridView dgvAtributos;
        private Button btnAgregarAtributo;
        private Button btnEliminarAtributo;
        private Button btnGuardar;
        private Button btnCancelar;

        // Propiedades públicas
        public string NombreNodo { get; private set; }
        public string ValorNodo { get; private set; }
        public Dictionary<string, string> Atributos { get; private set; }

        public NodoForm(string titulo, string nombre, string valor, Dictionary<string, string> atributos)
        {
            this.Text = titulo;
            this.Size = new System.Drawing.Size(400, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int y = 12;

            // Nombre del nodo
            lblNombre = new Label();
            lblNombre.Text = "Nombre del nodo:";
            lblNombre.Location = new System.Drawing.Point(12, y);
            lblNombre.AutoSize = true;
            y += 18;

            txtNombre = new TextBox();
            txtNombre.Location = new System.Drawing.Point(12, y);
            txtNombre.Size = new System.Drawing.Size(360, 22);
            txtNombre.Text = nombre;
            y += 30;

            // Valor del nodo
            lblValor = new Label();
            lblValor.Text = "Valor del nodo (opcional):";
            lblValor.Location = new System.Drawing.Point(12, y);
            lblValor.AutoSize = true;
            y += 18;

            txtValor = new TextBox();
            txtValor.Location = new System.Drawing.Point(12, y);
            txtValor.Size = new System.Drawing.Size(360, 22);
            txtValor.Text = valor;
            y += 30;

            // Atributos
            lblAtributos = new Label();
            lblAtributos.Text = "Atributos (nombre = valor):";
            lblAtributos.Location = new System.Drawing.Point(12, y);
            lblAtributos.AutoSize = true;
            y += 20;

            dgvAtributos = new DataGridView();
            dgvAtributos.Location = new System.Drawing.Point(12, y);
            dgvAtributos.Size = new System.Drawing.Size(360, 130);
            dgvAtributos.AllowUserToAddRows = false;
            dgvAtributos.MultiSelect = false;
            dgvAtributos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAtributos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvAtributos.RowHeadersVisible = false;

            DataGridViewTextBoxColumn colNombre = new DataGridViewTextBoxColumn();
            colNombre.HeaderText = "Nombre";
            colNombre.Width = 170;

            DataGridViewTextBoxColumn colValor = new DataGridViewTextBoxColumn();
            colValor.HeaderText = "Valor";
            colValor.Width = 170;

            dgvAtributos.Columns.Add(colNombre);
            dgvAtributos.Columns.Add(colValor);

            // Cargar atributos existentes
            foreach (var attr in atributos)
                dgvAtributos.Rows.Add(attr.Key, attr.Value);

            y += 135;

            btnAgregarAtributo = new Button();
            btnAgregarAtributo.Text = "Agregar atributo";
            btnAgregarAtributo.Location = new System.Drawing.Point(12, y);
            btnAgregarAtributo.Size = new System.Drawing.Size(130, 26);
            btnAgregarAtributo.Click += (s, e) => dgvAtributos.Rows.Add("", "");
            y += 34;

            btnEliminarAtributo = new Button();
            btnEliminarAtributo.Text = "Eliminar atributo";
            btnEliminarAtributo.Location = new System.Drawing.Point(152, y - 34);
            btnEliminarAtributo.Size = new System.Drawing.Size(130, 26);
            btnEliminarAtributo.Click += BtnEliminarAtributo_Click;

            // Botones guardar / cancelar
            btnGuardar = new Button();
            btnGuardar.Text = "Guardar";
            btnGuardar.Location = new System.Drawing.Point(200, y);
            btnGuardar.Size = new System.Drawing.Size(80, 28);
            btnGuardar.Click += BtnGuardar_Click;

            btnCancelar = new Button();
            btnCancelar.Text = "Cancelar";
            btnCancelar.Location = new System.Drawing.Point(292, y);
            btnCancelar.Size = new System.Drawing.Size(80, 28);
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[]
            {
                lblNombre, txtNombre,
                lblValor, txtValor,
                lblAtributos, dgvAtributos,
                btnAgregarAtributo, btnEliminarAtributo,
                btnGuardar, btnCancelar
            });

            // Ajustar altura del formulario
            this.ClientSize = new System.Drawing.Size(392, y + 40);

            this.AcceptButton = btnGuardar;
            this.CancelButton = btnCancelar;
        }

        private void BtnEliminarAtributo_Click(object sender, EventArgs e)
        {
            if (dgvAtributos.SelectedRows.Count > 0)
                dgvAtributos.Rows.RemoveAt(dgvAtributos.SelectedRows[0].Index);
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del nodo es obligatorio.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            // Validar nombre XML
            try { System.Xml.XmlConvert.VerifyName(txtNombre.Text.Trim()); }
            catch
            {
                MessageBox.Show("El nombre del nodo no es válido para XML.\nEvita espacios y caracteres especiales.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            NombreNodo = txtNombre.Text.Trim();
            ValorNodo = txtValor.Text;
            Atributos = new Dictionary<string, string>();

            foreach (DataGridViewRow fila in dgvAtributos.Rows)
            {
                string clave = fila.Cells[0].Value?.ToString()?.Trim() ?? "";
                string val = fila.Cells[1].Value?.ToString() ?? "";

                if (string.IsNullOrEmpty(clave)) continue;

                try { System.Xml.XmlConvert.VerifyName(clave); }
                catch
                {
                    MessageBox.Show($"El atributo \"{clave}\" no es un nombre XML válido.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!Atributos.ContainsKey(clave))
                    Atributos[clave] = val;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}