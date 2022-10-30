using System;


/// <summary>
/// Abstraction to allow a display control to show status text updates
/// </summary>
interface IUpdateStatusText
{
    void SetStatusText(string text);
}
