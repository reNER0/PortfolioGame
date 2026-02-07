using System;

[Serializable]
public class JoinQueueResponse
{
    public string ticketId;
}

[Serializable]
public class QueueStatusResponse
{
    public string status;
    public string matchId;
    public ConnectInfo connect;
}

[Serializable]
public class ConnectInfo
{
    public string ip;
    public int port;
    public string token;
}
