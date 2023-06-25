using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows.Forms;

namespace ResearchQuery
{
    /// <summary>
    /// The form for all querying of the EMMA Database.
    /// </summary>
    public partial class QueryForm : Form
    {
        private Controller controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryForm"/> class.
        /// </summary>
        public QueryForm()
        {
            this.InitializeComponent();

            // connect to the EMMA Backend server.
            this.controller = new Controller();
            this.controller.ConnectToDatabase("localhost", "root", "root");
            this.controller.LoadFilteringData();

            // get all studies in the EMMA Backend database.
            this.InitializeStudyListBox();

            this.InitializeCalculationTableView();

            // enable double-buffering for calculation table display
            this.CurrentCalculationTableView.GetType()?.
                GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.
                SetValue(this.CurrentCalculationTableView, true, null);

            // apply these optiosn from improved performance
            this.CurrentCalculationTableView.RowHeadersVisible = false;
            this.CurrentCalculationTableView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
        }

        private void InitializeCalculationTableView()
        {
            /*// add all columns in the calcultion table
            foreach (string column_name in this.controller.GetCalculationTableColumns())
            {
                this.CurrentCalculationTableView.Columns.Add(column_name, column_name);
            }

            // add 50 empty rows
            for (int i = 0; i < 50; i++)
            {
                this.CurrentCalculationTableView.Rows.Add();
            }*/
        }

        private void InitializeStudyListBox()
        {
            string[] studies = this.controller.GetStudies();

            foreach (string study in studies)
            {
                this.StudyCheckListBox.Items.Add(study);
            }
        }

        private void StudyCheckListBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(CheckedListBox))
            {
                string[] selected_studies = new string[((CheckedListBox)sender).CheckedItems.Count];
                int index = 0;

                // clear cohort options and if nothing is selected leave it cleared
                this.CohortSelectionView.Rows.Clear();
                this.ViewCalculationTableButton.Enabled = false;
                if (selected_studies.Length == 0)
                {
                    return;
                }

                // get the studies selected for further querying
                foreach (string study_name in ((CheckedListBox)sender).CheckedItems)
                {
                    selected_studies[index++] = study_name;
                }

                // update the cohort checklist
                this.ResetCohortSelections(this.controller.GetCohorts(selected_studies));
            }
        }

        private void ResetCohortSelections(KeyValuePair<string, int>[] study_cohort_pairs)
        {
            int index = 0;
            foreach (KeyValuePair<string, int> study_cohort_pair in study_cohort_pairs)
            {
                index = this.CohortSelectionView.Rows.Add();
                this.CohortSelectionView.Rows[index].Cells["StudyOptionColumn"].Value = study_cohort_pair.Key;
                this.CohortSelectionView.Rows[index].Cells["StudyOptionColumn"].ReadOnly = true;

                this.CohortSelectionView.Rows[index].Cells["CohortSelectionColumn"].Value = study_cohort_pair.Value;
                this.CohortSelectionView.Rows[index].Cells["CohortSelectionColumn"].ReadOnly = true;
            }

            this.CohortSelectionView.ClearSelection();
        }

        private void CohortSelectionView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // updates checkbox if click a row, or the checkbox directly.
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex > 0)
                {
                    var check = this.CohortSelectionView.Rows[e.RowIndex].Cells["CheckCohortColumn"].Value;
                    if (check is null)
                    {
                        this.CohortSelectionView.Rows[e.RowIndex].Cells["CheckCohortColumn"].Value = true;
                    }
                    else
                    {
                        if ((bool)check)
                        {
                            this.CohortSelectionView.Rows[e.RowIndex].Cells["CheckCohortColumn"].Value = false;
                        }
                        else
                        {
                            this.CohortSelectionView.Rows[e.RowIndex].Cells["CheckCohortColumn"].Value = true;
                        }
                    }
                }
            }
        }

        private void CohortSelectionView_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            foreach (DataGridViewRow cohort_row in this.CohortSelectionView.Rows)
            {

                if (Convert.ToBoolean(cohort_row.Cells["CheckCohortColumn"].Value))
                {
                    this.ViewCalculationTableButton.Enabled = true;
                    return;
                }
            }
        }

        private void ViewCalculationTableButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.ViewCalculationTableButton.Enabled)
            {
                List<KeyValuePair<string, int>> selected_cohorts = new List<KeyValuePair<string, int>>();
                foreach (DataGridViewRow cohort_row in this.CohortSelectionView.Rows)
                {
                    if (Convert.ToBoolean(cohort_row.Cells["CheckCohortColumn"].Value))
                    {
                        string study = (string)cohort_row.Cells["StudyOptionColumn"].Value;
                        int cohort = (int)cohort_row.Cells["CohortSelectionColumn"].Value;

                        selected_cohorts.Add(new KeyValuePair<string, int>(study, cohort));
                    }
                }

                this.ShowNewCalculationTable(this.controller.GetCalculationTable(selected_cohorts.ToArray()));
            }
        }

        private void ShowNewCalculationTable(DataTable? calculation_table)
        {
            this.CurrentCalculationTableView.DataSource = calculation_table;
        }

        private void CohortSelectionView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}