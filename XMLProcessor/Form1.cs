using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace XMLProcessor
{
    public class Form1 : Form
    {
        // Controles
        private Button btnNuevo;
        private Button btnCargar;
        private Button btnGuardar;
        private Button btnAgregarNodo;
        private Button btnEditarNodo;
        private Button btnEliminarNodo;
        private Button btnExpandir;
        private Button btnColapsar;
        private Label lblArchivo;
        private Label lblStatus;
        private TreeView treeXML;
        private Panel panelBotones;
        private Panel panelAcciones;

        private XDocument documentoXML = null;
        private string rutaArchivoActual = string.Empty;

        public Form1()
        {
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            this.Text = "Procesador de Archivos XML";
            this.Size = new System.Drawing.Size(850, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(600, 450);

            // Panel botones principales
            panelBotones = new Panel();
            panelBotones.Dock = DockStyle.Top;
            panelBotones.Height = 45;
            panelBotones.Padding = new Padding(5);

            btnNuevo = new Button();
            btnNuevo.Text = "Nuevo XML";
            btnNuevo.Location = new System.Drawing.Point(5, 8);
            btnNuevo.Size = new System.Drawing.Size(90, 28);
            btnNuevo.Click += BtnNuevo_Click;

            btnCargar = new Button();
            btnCargar.Text = "Cargar XML";
            btnCargar.Location = new System.Drawing.Point(105, 8);
            btnCargar.Size = new System.Drawing.Size(90, 28);
            btnCargar.Click += BtnCargar_Click;

            btnGuardar = new Button();
            btnGuardar.Text = "Guardar XML";
            btnGuardar.Location = new System.Drawing.Point(205, 8);
            btnGuardar.Size = new System.Drawing.Size(90, 28);
            btnGuardar.Enabled = false;
            btnGuardar.Click += BtnGuardar_Click;

            btnExpandir = new Button();
            btnExpandir.Text = "Expandir";
            btnExpandir.Location = new System.Drawing.Point(310, 8);
            btnExpandir.Size = new System.Drawing.Size(75, 28);
            btnExpandir.Enabled = false;
            btnExpandir.Click += (s, e) => { treeXML.ExpandAll(); };

            btnColapsar = new Button();
            btnColapsar.Text = "Colapsar";
            btnColapsar.Location = new System.Drawing.Point(395, 8);
            btnColapsar.Size = new System.Drawing.Size(75, 28);
            btnColapsar.Enabled = false;
            btnColapsar.Click += (s, e) => { treeXML.CollapseAll(); };

            panelBotones.Controls.AddRange(new Control[] { btnNuevo, btnCargar, btnGuardar, btnExpandir, btnColapsar });

            // Panel acciones nodo
            panelAcciones = new Panel();
            panelAcciones.Dock = DockStyle.Top;
            panelAcciones.Height = 42;
            panelAcciones.Padding = new Padding(5);

            Label lblAcciones = new Label();
            lblAcciones.Text = "Nodo seleccionado:";
            lblAcciones.Location = new System.Drawing.Point(5, 13);
            lblAcciones.AutoSize = true;

            btnAgregarNodo = new Button();
            btnAgregarNodo.Text = "Agregar hijo";
            btnAgregarNodo.Location = new System.Drawing.Point(125, 8);
            btnAgregarNodo.Size = new System.Drawing.Size(95, 28);
            btnAgregarNodo.Enabled = false;
            btnAgregarNodo.Click += BtnAgregarNodo_Click;

            btnEditarNodo = new Button();
            btnEditarNodo.Text = "Editar";
            btnEditarNodo.Location = new System.Drawing.Point(230, 8);
            btnEditarNodo.Size = new System.Drawing.Size(75, 28);
            btnEditarNodo.Enabled = false;
            btnEditarNodo.Click += BtnEditarNodo_Click;

            btnEliminarNodo = new Button();
            btnEliminarNodo.Text = "Eliminar";
            btnEliminarNodo.Location = new System.Drawing.Point(315, 8);
            btnEliminarNodo.Size = new System.Drawing.Size(75, 28);
            btnEliminarNodo.Enabled = false;
            btnEliminarNodo.Click += BtnEliminarNodo_Click;

            panelAcciones.Controls.AddRange(new Control[] { lblAcciones, btnAgregarNodo, btnEditarNodo, btnEliminarNodo });

            // Label archivo
            lblArchivo = new Label();
            lblArchivo.Dock = DockStyle.Top;
            lblArchivo.Height = 22;
            lblArchivo.Padding = new Padding(5, 3, 0, 0);
            lblArchivo.Text = "Ningún archivo cargado";

            // TreeView
            treeXML = new TreeView();
            treeXML.Dock = DockStyle.Fill;
            treeXML.HideSelection = false;
            treeXML.AfterSelect += TreeXML_AfterSelect;
            treeXML.NodeMouseDoubleClick += TreeXML_NodeMouseDoubleClick;

            // Label status
            lblStatus = new Label();
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = 22;
            lblStatus.Padding = new Padding(5, 3, 0, 0);
            lblStatus.Text = "Listo";
            lblStatus.BorderStyle = BorderStyle.Fixed3D;

            this.Controls.Add(treeXML);
            this.Controls.Add(panelAcciones);
            this.Controls.Add(lblArchivo);
            this.Controls.Add(panelBotones);
            this.Controls.Add(lblStatus);
        }

        // ─── NUEVO XML ────────────────────────────────────────────────────────────
        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            using (NodoForm form = new NodoForm("Crear raíz del XML", "", "", new Dictionary<string, string>()))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    XElement raiz = new XElement(form.NombreNodo);
                    if (!string.IsNullOrWhiteSpace(form.ValorNodo))
                        raiz.Value = form.ValorNodo;
                    foreach (var attr in form.Atributos)
                        raiz.SetAttributeValue(attr.Key, attr.Value);

                    documentoXML = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), raiz);
                    rutaArchivoActual = string.Empty;
                    lblArchivo.Text = "Nuevo documento XML (sin guardar)";
                    CargarArbolDesdeDocumento();
                    HabilitarControles(true);
                    lblStatus.Text = "Nuevo documento XML creado.";
                }
            }
        }

        // ─── CARGAR XML ───────────────────────────────────────────────────────────
        private void BtnCargar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Archivos XML (*.xml)|*.xml|Todos los archivos (*.*)|*.*";
                ofd.Title = "Seleccionar archivo XML";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        documentoXML = XDocument.Load(ofd.FileName);
                        rutaArchivoActual = ofd.FileName;
                        lblArchivo.Text = $"Archivo: {rutaArchivoActual}";
                        CargarArbolDesdeDocumento();
                        HabilitarControles(true);
                        lblStatus.Text = $"XML cargado correctamente.";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al cargar el archivo:\n{ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ─── GUARDAR XML ──────────────────────────────────────────────────────────
        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Archivos XML (*.xml)|*.xml";
                sfd.Title = "Guardar archivo XML";
                sfd.FileName = string.IsNullOrEmpty(rutaArchivoActual)
                    ? "documento.xml"
                    : Path.GetFileName(rutaArchivoActual);

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        XmlWriterSettings settings = new XmlWriterSettings
                        {
                            Indent = true,
                            IndentChars = "  ",
                            Encoding = Encoding.UTF8
                        };

                        using (XmlWriter writer = XmlWriter.Create(sfd.FileName, settings))
                            documentoXML.Save(writer);

                        rutaArchivoActual = sfd.FileName;
                        lblArchivo.Text = $"Archivo: {rutaArchivoActual}";
                        lblStatus.Text = $"Guardado: {sfd.FileName}";
                        MessageBox.Show("Archivo guardado exitosamente.", "Éxito",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al guardar:\n{ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ─── AGREGAR NODO HIJO ────────────────────────────────────────────────────
        private void BtnAgregarNodo_Click(object sender, EventArgs e)
        {
            if (treeXML.SelectedNode == null) return;

            XElement padre = (XElement)treeXML.SelectedNode.Tag;

            using (NodoForm form = new NodoForm("Agregar nodo hijo", "", "", new Dictionary<string, string>()))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    XElement hijo = new XElement(form.NombreNodo);
                    if (!string.IsNullOrWhiteSpace(form.ValorNodo))
                        hijo.Value = form.ValorNodo;
                    foreach (var attr in form.Atributos)
                        hijo.SetAttributeValue(attr.Key, attr.Value);

                    padre.Add(hijo);
                    CargarArbolDesdeDocumento();
                    lblStatus.Text = $"Nodo <{form.NombreNodo}> agregado.";
                }
            }
        }

        // ─── EDITAR NODO ──────────────────────────────────────────────────────────
        private void BtnEditarNodo_Click(object sender, EventArgs e)
        {
            if (treeXML.SelectedNode == null) return;
            EditarNodoSeleccionado();
        }

        private void EditarNodoSeleccionado()
        {
            XElement nodo = (XElement)treeXML.SelectedNode.Tag;

            // Leer atributos actuales
            Dictionary<string, string> attrActuales = nodo.Attributes()
                .ToDictionary(a => a.Name.LocalName, a => a.Value);

            // Solo pasar el valor si no tiene hijos elemento
            string valorActual = nodo.HasElements ? "" : nodo.Value;

            using (NodoForm form = new NodoForm("Editar nodo", nodo.Name.LocalName, valorActual, attrActuales))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // Renombrar si cambió el nombre
                    if (form.NombreNodo != nodo.Name.LocalName)
                        nodo.Name = form.NombreNodo;

                    // Actualizar valor (solo si no tiene hijos elemento)
                    if (!nodo.HasElements)
                        nodo.Value = form.ValorNodo;

                    // Actualizar atributos
                    foreach (var attr in nodo.Attributes().ToList())
                        attr.Remove();
                    foreach (var attr in form.Atributos)
                        nodo.SetAttributeValue(attr.Key, attr.Value);

                    CargarArbolDesdeDocumento();
                    lblStatus.Text = $"Nodo <{form.NombreNodo}> actualizado.";
                }
            }
        }

        // ─── ELIMINAR NODO ────────────────────────────────────────────────────────
        private void BtnEliminarNodo_Click(object sender, EventArgs e)
        {
            if (treeXML.SelectedNode == null) return;

            XElement nodo = (XElement)treeXML.SelectedNode.Tag;

            if (nodo.Parent == null)
            {
                MessageBox.Show("No se puede eliminar el nodo raíz.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"¿Eliminar el nodo <{nodo.Name.LocalName}> y todos sus hijos?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string nombre = nodo.Name.LocalName;
                nodo.Remove();
                CargarArbolDesdeDocumento();
                lblStatus.Text = $"Nodo <{nombre}> eliminado.";
            }
        }

        // ─── CARGAR ÁRBOL DESDE DOCUMENTO ────────────────────────────────────────
        private void CargarArbolDesdeDocumento()
        {
            treeXML.BeginUpdate();
            treeXML.Nodes.Clear();

            if (documentoXML?.Root != null)
            {
                TreeNode raiz = CrearNodoArbol(documentoXML.Root);
                treeXML.Nodes.Add(raiz);
                treeXML.ExpandAll();
            }

            treeXML.EndUpdate();
        }

        private TreeNode CrearNodoArbol(XElement elemento)
        {
            string etiqueta = $"<{elemento.Name.LocalName}>";

            // Mostrar atributos en la etiqueta
            foreach (XAttribute attr in elemento.Attributes())
                etiqueta += $" {attr.Name.LocalName}=\"{attr.Value}\"";

            // Mostrar valor si no tiene hijos elemento
            if (!elemento.HasElements && !string.IsNullOrWhiteSpace(elemento.Value))
                etiqueta += $" = {elemento.Value}";

            TreeNode nodo = new TreeNode(etiqueta);
            nodo.Tag = elemento;

            foreach (XElement hijo in elemento.Elements())
                nodo.Nodes.Add(CrearNodoArbol(hijo));

            return nodo;
        }

        // ─── SELECCIÓN EN ÁRBOL ───────────────────────────────────────────────────
        private void TreeXML_AfterSelect(object sender, TreeViewEventArgs e)
        {
            bool haySeleccion = e.Node != null;
            btnAgregarNodo.Enabled = haySeleccion;
            btnEditarNodo.Enabled = haySeleccion;
            btnEliminarNodo.Enabled = haySeleccion;

            if (haySeleccion)
            {
                XElement nodo = (XElement)e.Node.Tag;
                lblStatus.Text = $"Nodo: <{nodo.Name.LocalName}>  |  Hijos: {nodo.Elements().Count()}  |  Atributos: {nodo.Attributes().Count()}";
            }
        }

        private void TreeXML_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            EditarNodoSeleccionado();
        }

        // ─── HABILITAR CONTROLES ──────────────────────────────────────────────────
        private void HabilitarControles(bool estado)
        {
            btnGuardar.Enabled = estado;
            btnExpandir.Enabled = estado;
            btnColapsar.Enabled = estado;
        }

        // Note: application entry point is defined in Program.cs
    }
}