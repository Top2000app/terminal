using Top2000.Features.AllEditions;

namespace Top2000.Apps.Teminal.Views.SelectEdition;

public class SelectEditionDialog : Dialog
{
    private readonly EditionsDataSource editionDataSource;
    private readonly TableView editionsTable;
    private readonly SortedSet<Edition> editions;
    private TaskCompletionSource<bool>? taskCompletionSource;

    public SelectEditionDialog(SortedSet<Edition> editions)
    {
        this.editionDataSource = new(editions);

        this.editionsTable = new(this.editionDataSource)
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Height = this.editionDataSource.Rows,
            Width = 36,
            MultiSelect = false,
            FullRowSelect = false,
            Style = new TableStyle
            {
                ShowHorizontalScrollIndicators = false,
                ShowHorizontalBottomline = false,
                ShowHeaders = false,
                ExpandLastColumn = false,
                ShowHorizontalHeaderOverline = false,
                ShowVerticalHeaderLines = false,
                ShowHorizontalHeaderUnderline = false,
                ShowVerticalCellLines = false,
            }
        };

        this.Add(this.editionsTable);

        var okButton = new Button { Text = "_Show" };
        okButton.Accept += (s, e) =>
        {
            taskCompletionSource!.SetResult(true);
            Application.RequestStop();
        };

        var cancelButton = new Button { Text = "_Cancel" };
        cancelButton.Accept += (s, e) =>
        {
            taskCompletionSource!.SetResult(false);
            Application.RequestStop();
        };

        this.Title = "Selecteer editie";
        this.Width = 40;
        this.Height = this.editionDataSource.Rows * 2;
        this.Buttons = [okButton, cancelButton];
        this.ButtonAlignment = Alignment.Center;
        this.editions = editions;
    }


    public async Task<Edition> ShowDialogAsync(Edition currentEdition)
    {
        this.taskCompletionSource = new TaskCompletionSource<bool>();

        Application.Run(this);

        var clickedOk = await this.taskCompletionSource.Task;
        if (clickedOk)
        {
            var selectedYear = int.Parse(this.editionDataSource[editionsTable.SelectedRow, editionsTable.SelectedColumn].ToString()!.Trim());

            return editions.First(x => x.Year == selectedYear);
        }

        return currentEdition;
    }
}
