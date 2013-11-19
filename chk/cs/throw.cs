using System;

static class Program
{
    static void Main()
    {
        try
        {
             throw new Exception();
        }
        catch (Exception)
        {
        }
    }
}
