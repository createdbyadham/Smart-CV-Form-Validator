using System;
using System.Drawing;
using System.Text;  // Add this for StringBuilder
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Cyber2
{
    public partial class Form1 : Form
    {
        // Regular expressions for validation
        private readonly string namePattern = @"^[a-zA-Z\s]{2,50}$";
        private readonly string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        private readonly string phonePattern = @"^\+?[\d\s-]{10,15}$";
        private readonly string postalCodePattern = @"^\d{5}(-\d{4})?$";
        
        public Form1()
        {
            InitializeComponent();
            SetupForm();
        }
        
        private void SetupForm()
        {
            // Form properties
            this.Text = "Smart CV & Form Validator";
            this.Size = new Size(800, 600);
    
            // Create tab control
            TabControl tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
    
            // Manual Form Tab
            TabPage manualFormTab = new TabPage("Manual Form Entry");
            SetupManualFormTab(manualFormTab);
            tabControl.TabPages.Add(manualFormTab);
    
            // CV Parser Tab
            TabPage cvParserTab = new TabPage("CV Parser");
            SetupCVParserTab(cvParserTab);
            tabControl.TabPages.Add(cvParserTab);
    
            this.Controls.Add(tabControl);
        }
    
        private void SetupManualFormTab(TabPage tab)
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.ColumnCount = 3;
            panel.RowCount = 8; // Increased row count for save buttons
            panel.Padding = new Padding(10);
    
            // Add form fields
            AddFormField(panel, "Name:", new TextBox(), 0);
            AddFormField(panel, "Email:", new TextBox(), 1);
            AddFormField(panel, "Phone:", new TextBox(), 2);
            AddFormField(panel, "Password:", new TextBox() { UseSystemPasswordChar = true }, 3);
            AddFormField(panel, "Address:", new TextBox(), 4);
            AddFormField(panel, "Postal Code:", new TextBox(), 5);
    
            Button validateButton = new Button();
            validateButton.Text = "Validate";
            validateButton.Click += ValidateButton_Click;
            panel.Controls.Add(validateButton, 1, 6);
            
            // Add save buttons
            FlowLayoutPanel saveButtonsPanel = new FlowLayoutPanel();
            saveButtonsPanel.FlowDirection = FlowDirection.LeftToRight;
            saveButtonsPanel.AutoSize = true;
            
            Button saveTxtButton = new Button();
            saveTxtButton.Text = "Save as TXT";
            saveTxtButton.Click += (s, e) => SaveFormData(panel, "txt");
            saveButtonsPanel.Controls.Add(saveTxtButton);
            
            Button saveCsvButton = new Button();
            saveCsvButton.Text = "Save as CSV";
            saveCsvButton.Click += (s, e) => SaveFormData(panel, "csv");
            saveButtonsPanel.Controls.Add(saveCsvButton);
            
            panel.Controls.Add(saveButtonsPanel, 1, 7);
    
            tab.Controls.Add(panel);
        }
    
        private void SetupCVParserTab(TabPage tab)
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.ColumnCount = 2;
            panel.RowCount = 4; // Increased row count for save buttons
            panel.Padding = new Padding(10);
            
            // Set column styles to make the text boxes use full width
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
    
            TextBox cvTextBox = new TextBox();
            cvTextBox.Multiline = true;
            cvTextBox.ScrollBars = ScrollBars.Vertical;
            cvTextBox.Height = 200;
            cvTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            panel.Controls.Add(cvTextBox, 0, 0);
            panel.SetColumnSpan(cvTextBox, 2);
    
            Button parseButton = new Button();
            parseButton.Text = "Parse CV";
            parseButton.Click += ParseCVButton_Click;
            panel.Controls.Add(parseButton, 0, 1);
    
            TextBox resultsBox = new TextBox();
            resultsBox.Multiline = true;
            resultsBox.ReadOnly = true;
            resultsBox.ScrollBars = ScrollBars.Vertical;
            resultsBox.Height = 200;
            resultsBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            panel.Controls.Add(resultsBox, 0, 2);
            panel.SetColumnSpan(resultsBox, 2);
            
            // Add save buttons
            FlowLayoutPanel saveButtonsPanel = new FlowLayoutPanel();
            saveButtonsPanel.FlowDirection = FlowDirection.LeftToRight;
            saveButtonsPanel.AutoSize = true;
            
            Button saveTxtButton = new Button();
            saveTxtButton.Text = "Save as TXT";
            saveTxtButton.Click += (s, e) => SaveCVResults(resultsBox.Text, "txt");
            saveButtonsPanel.Controls.Add(saveTxtButton);
            
            Button saveCsvButton = new Button();
            saveCsvButton.Text = "Save as CSV";
            saveCsvButton.Click += (s, e) => SaveCVResults(resultsBox.Text, "csv");
            saveButtonsPanel.Controls.Add(saveCsvButton);
            
            panel.Controls.Add(saveButtonsPanel, 0, 3);
    
            tab.Controls.Add(panel);
        }
    
        private void AddFormField(TableLayoutPanel panel, string label, TextBox textBox, int row)
        {
            panel.Controls.Add(new Label() { Text = label }, 0, row);
            panel.Controls.Add(textBox, 1, row);
            panel.Controls.Add(new Label(), 2, row); // For validation message
        }
    
        private void ValidateButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button button) return;
            TableLayoutPanel? panel = button.Parent as TableLayoutPanel;
            if (panel == null) return;
            bool isValid = true;
    
            // Validate each field
            foreach (Control control in panel.Controls)
            {
                if (control is TextBox textBox)
                {
                    Control? labelControl = panel.GetControlFromPosition(0, panel.GetRow(textBox));
                    if (labelControl == null) continue;
                    
                    string fieldName = labelControl.Text;
                    string value = textBox.Text;
                    string errorMessage = ValidateField(fieldName, value);
    
                    Label? messageLabel = panel.GetControlFromPosition(2, panel.GetRow(textBox)) as Label;
                    if (messageLabel == null) continue;
    
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        messageLabel.Text = errorMessage;
                        messageLabel.ForeColor = Color.Red;
                        textBox.BackColor = Color.LightPink;
                        isValid = false;
                    }
                    else
                    {
                        messageLabel.Text = "✓";
                        messageLabel.ForeColor = Color.Green;
                        textBox.BackColor = Color.White;
                    }
                }
            }
    
            if (isValid)
            {
                MessageBox.Show("All fields are valid!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    
        private void ParseCVButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button button) return;
            TableLayoutPanel? panel = button.Parent as TableLayoutPanel;
            if (panel == null) return;
    
            TextBox? cvTextBox = panel.GetControlFromPosition(0, 0) as TextBox;
            TextBox? resultsBox = panel.GetControlFromPosition(0, 2) as TextBox;
            
            if (cvTextBox == null || resultsBox == null) return;
    
            string cvText = cvTextBox.Text;
            string results = ParseCV(cvText);
            resultsBox.Text = results;
        }
    
        private string ValidateField(string fieldName, string value)
        {
            // Check if the field is empty first
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Field cannot be empty";
            }
            
            switch (fieldName.TrimEnd(':'))
            {
                case "Name":
                    return !Regex.IsMatch(value, namePattern) ? "Invalid name format (English or Arabic)" : "";
                case "Email":
                    return !Regex.IsMatch(value, emailPattern) ? "Invalid email format" : "";
                case "Phone":
                    return !Regex.IsMatch(value, phonePattern) ? "Invalid phone format" : "";
                case "Postal Code":
                    return !Regex.IsMatch(value, postalCodePattern) ? "Invalid postal code" : "";
                default:
                    return "";
            }
        }
    
        private string ParseCV(string cvText)
        {
            string namePattern = @"(?i)name:\s*([a-zA-Z\s]+)|([a-zA-Z]+\s[a-zA-Z\s]+)";
            string emailPattern = @"(?i)[\w\.-]+@[\w\.-]+\.\w+";
            string phonePattern = @"(?i)phone:?\s*(\+?\d[\d\s\(\)-]{10,})";
            string skillsPattern = @"(?i)skills[\s\n]*([^.]*?)(?=\n\n|$|Experience|Education)";
            string experiencePattern = @"(?i)Experience[\s\n]*(.*?)(?=\n\n|$|Education)";
            string yearsPattern = @"(?i)(\d+)\s*(?:years?|yrs?)(?:\s+of\s+)?(?:experience)?";
            string jobTitlePattern = @"(?i)([\w\s]+)\s*[—–-]\s*([\w\s]+)\s*\n[\s\n]*([\w\s]+\s+\d{4}\s*[–-]\s*(?:Present|[\w\s]*\d{4}))";
    
            StringBuilder result = new StringBuilder();
            result.AppendLine("Extracted Information:");
            result.AppendLine("--------------------");
    
            // Extract name
            Match nameMatch = Regex.Match(cvText, namePattern);
            if (nameMatch.Success)
            {
                string name = nameMatch.Groups[1].Value.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    name = nameMatch.Groups[2].Value.Trim();
                }
                result.AppendLine($"Name: {name}");
            }
    
            // Extract email
            Match emailMatch = Regex.Match(cvText, emailPattern);
            if (emailMatch.Success)
            {
                result.AppendLine($"Email: {emailMatch.Value}");
            }
    
            // Extract phone
            Match phoneMatch = Regex.Match(cvText, phonePattern);
            if (phoneMatch.Success)
            {
                string phone = phoneMatch.Groups[1].Value.Trim();
                result.AppendLine($"Phone: {phone}");
            }
    
            // Extract skills
            Match skillsMatch = Regex.Match(cvText, skillsPattern, RegexOptions.Singleline);
            if (skillsMatch.Success)
            {
                string skillsText = skillsMatch.Groups[1].Value;
                var skills = skillsText
                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                
                if (skills.Any())
                {
                    result.AppendLine($"Skills: {string.Join(", ", skills)}");
                }
            }
    
            // Extract experience - years
            Match yearsMatch = Regex.Match(cvText, yearsPattern);
            if (yearsMatch.Success)
            {
                string years = yearsMatch.Value.Trim();
                result.AppendLine($"Experience: {years}");
            }
            
            // Extract job details
            Match jobMatch = Regex.Match(cvText, jobTitlePattern, RegexOptions.Singleline);
            if (jobMatch.Success)
            {
                string jobTitle = jobMatch.Groups[1].Value.Trim();
                string company = jobMatch.Groups[2].Value.Trim();
                string period = jobMatch.Groups[3].Value.Trim();
                result.AppendLine($"Job Title: {jobTitle}");
                result.AppendLine($"Company: {company}");
                result.AppendLine($"Period: {period}");
            }
            
            // Extract experience details
            Match expDetailsMatch = Regex.Match(cvText, experiencePattern, RegexOptions.Singleline);
            if (expDetailsMatch.Success)
            {
                string expDetails = expDetailsMatch.Groups[1].Value;
                var responsibilities = expDetails
                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s) && !Regex.IsMatch(s, @"^Experience|^Job Title|^Company|^Period"))
                    .ToList();
                
                if (responsibilities.Any())
                {
                    result.AppendLine("Responsibilities:");
                    foreach (var responsibility in responsibilities)
                    {
                        // Skip job title/company/period lines which we already extracted
                        if (!Regex.IsMatch(responsibility, jobTitlePattern))
                        {                            
                            result.AppendLine($"- {responsibility}");
                        }
                    }
                }
            }
    
            return result.ToString();
        }
    
        private void SaveToFile(string content, string fileType)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = fileType.ToLower() == "csv" ? 
                "CSV files (*.csv)|*.csv" : "Text files (*.txt)|*.txt";
            
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (fileType.ToLower() == "csv" && !content.Contains(","))
                {
                    // Convert to CSV format if not already in CSV format
                    content = ConvertToCSV(content);
                }
                
                System.IO.File.WriteAllText(saveFileDialog.FileName, content);
                MessageBox.Show("File saved successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        private string ConvertToCSV(string content)
        {
            StringBuilder csv = new StringBuilder();
            string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string line in lines)
            {
                if (line.Contains(":"))
                {
                    string[] parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        csv.AppendLine($"\"{parts[0].Trim()}\",\"{parts[1].Trim()}\"");
                    }
                    else
                    {
                        csv.AppendLine($"\"{line}\"");
                    }
                }
                else
                {
                    csv.AppendLine($"\"{line}\"");
                }
            }
            
            return csv.ToString();
        }
        
        private void SaveFormData(TableLayoutPanel panel, string fileType)
        {
            StringBuilder content = new StringBuilder();
            
            foreach (Control control in panel.Controls)
            {
                if (control is TextBox textBox)
                {
                    Control? labelControl = panel.GetControlFromPosition(0, panel.GetRow(textBox));
                    if (labelControl == null) continue;
                    
                    string fieldName = labelControl.Text.TrimEnd(':');
                    string value = textBox.Text;
                    
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (fileType.ToLower() == "csv")
                        {
                            content.AppendLine($"\"{fieldName}\",\"{value}\"");
                        }
                        else
                        {
                            content.AppendLine($"{fieldName}: {value}");
                        }
                    }
                }
            }
            
            SaveToFile(content.ToString(), fileType);
        }
        
        private void SaveCVResults(string results, string fileType)
        {
            SaveToFile(results, fileType);
        }
    }
}