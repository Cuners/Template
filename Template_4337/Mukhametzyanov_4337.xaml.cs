using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using exc = Microsoft.Office.Interop.Excel;
namespace Template_4337
{
    /// <summary>
    /// Логика взаимодействия для Mukhametzyanov_4337.xaml
    /// </summary>
    public partial class Mukhametzyanov_4337 : Window
    {
        public Mukhametzyanov_4337()
        {
            InitializeComponent();
        }

        private void BnImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                DefaultExt = "*.xls;*.xlsx",
                Filter = "файл Excel (Spisok.xlsx)|*.xlsx",
                Title = "Выберите файл базы данных"
            };
            if (!(ofd.ShowDialog() == true))
                return;
            string[,] list;
            exc.Application ObjWorkExcel = new exc.Application();
            exc.Workbook ObjWorkBook = ObjWorkExcel.Workbooks.Open(ofd.FileName);
            exc.Worksheet ObjWorkSheet = (exc.Worksheet)ObjWorkBook.Sheets[1];
            var lastCell = ObjWorkSheet.Cells.SpecialCells(exc.XlCellType.xlCellTypeLastCell);
            int _columns = (int)lastCell.Column;
            int _rows = (int)lastCell.Row;
            list = new string[_rows, _columns];
            for (int j = 0; j < _columns; j++)
                for (int i = 0; i < _rows; i++)
                    list[i, j] = ObjWorkSheet.Cells[i + 1, j + 1].Text;
            ObjWorkBook.Close(false, Type.Missing, Type.Missing);
            ObjWorkExcel.Quit();
            GC.Collect();
            using (forisrpEntities1 forisrpEntities1 = new forisrpEntities1())
            {
                for (int i = 0; i < _rows; i++)
                {
                    forisrpEntities1.Rabochie.Add(new Rabochie() { ID = Convert.ToInt32(list[i, 0]), PIO = list[i, 1], Login = list[i, 2] });
                }
                forisrpEntities1.SaveChanges();
            }
        }

        private void BnExport_Click(object sender, RoutedEventArgs e)
        {
            List<Rabochie> allRabochie;
            List<Dolzhnosti> allDolzhostis;
            using (forisrpEntities1 forisrpEntities1 = new forisrpEntities1())
            {
                allRabochie = forisrpEntities1.Rabochie.ToList().OrderBy(d => d.PIO).ToList();
                allDolzhostis = forisrpEntities1.Dolzhnosti.ToList().OrderBy(d => d.Dolzhnost).ToList();

            }
            var app = new exc.Application();
            app.SheetsInNewWorkbook = allRabochie.Count();
            exc.Workbook workbook = app.Workbooks.Add(Type.Missing);
            var rabochieCategories = allRabochie.GroupBy(s => s.Dolzhnosti.Id).ToList();
            for (int i = 0; i < allDolzhostis.Count(); i++)
            {
                int startRowIndex = 1;
                exc.Worksheet worksheet = app.Worksheets.Item[i + 1];
                worksheet.Name = Convert.ToString(allRabochie[i].ID);
                worksheet.Cells[1][2] = "Порядковый номер";
                worksheet.Cells[2][2] = "ФИО работника";
                worksheet.Cells[3][3] = "Логин";
                startRowIndex++;
                foreach (var rabochie in rabochieCategories)
                {
                    if (rabochie.Key == allDolzhostis[i].Id)
                    {
                        exc.Range headerRange = worksheet.Range[worksheet.Cells[1][1], worksheet.Cells[3][1]];
                        headerRange.Merge();
                        headerRange.Value = allDolzhostis[i].Dolzhnost;
                        headerRange.HorizontalAlignment = exc.XlHAlign.xlHAlignCenter;
                        headerRange.Font.Italic = true;
                        startRowIndex++;
                        foreach (Rabochie rabochie1 in allRabochie)
                        {
                            if (rabochie1.DolzhnostId == rabochie.Key)
                            {
                                worksheet.Cells[1][startRowIndex] = rabochie1.ID;
                                worksheet.Cells[2][startRowIndex] = rabochie1.PIO;
                                worksheet.Cells[3][startRowIndex] = rabochie1.Login;
                                startRowIndex++;
                            }
                        }
                        worksheet.Cells[1][startRowIndex].Formula = $"=СЧЁТ(A3:A{startRowIndex - 1})";
                        worksheet.Cells[1][startRowIndex].Font.Bold = true;
                    }
                    else
                    {
                        continue;
                    }
                }
                exc.Range rangeBorders =
                worksheet.Range[worksheet.Cells[1][1], worksheet.Cells[3][startRowIndex - 1]];
                rangeBorders.Borders[exc.XlBordersIndex.xlEdgeBottom].LineStyle = rangeBorders.Borders[exc.XlBordersIndex.xlEdgeLeft].LineStyle = rangeBorders.Borders[exc.XlBordersIndex.xlEdgeTop].LineStyle = rangeBorders.Borders[exc.XlBordersIndex.xlEdgeRight].LineStyle = rangeBorders.Borders[exc.XlBordersIndex.xlInsideHorizontal].LineStyle =
                rangeBorders.Borders[exc.XlBordersIndex.xlInsideVertical].LineStyle = exc.XlLineStyle.xlContinuous;
                worksheet.Columns.AutoFit();
            }
            app.Visible = true;
        }
    }
}
