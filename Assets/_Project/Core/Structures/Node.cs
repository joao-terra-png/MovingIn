public class Node
{
    private object info;
    private Node next;

    public object GetInfo()
    {
        return info;
    }

    public Node GetNext()
    {
        return next;
    }

    public void SetInfo(object info)
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