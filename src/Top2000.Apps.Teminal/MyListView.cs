using Terminal.Gui;

public class MyListView : ListView
{
    public MyListView() : base()
    {
        
        
        base.AddCommand(Command.ScrollUp, () => ScrollVertical(-2));
        base.AddCommand(Command.ScrollDown, () => ScrollVertical(2));
       
        base.AddCommand(Command.ScrollLeft, () => ScrollHorizontal(-2));
        base.AddCommand(Command.ScrollRight, () => ScrollHorizontal(2));
        
    }
}