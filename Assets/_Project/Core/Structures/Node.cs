public class Node
{
    private static Object info;
    private Node next;

    public static Object GetInfo()
    {
        return info;
    }
    public Node GetNext()
    {
        return next;
    }
    public void SetInfo(Object info)
    {
        this.info = info;
    }
    public void SetNext(Node next)
    {
        this.next = next;
    }
    public Node()
    {
        info = null;
        next = null;
    }
}