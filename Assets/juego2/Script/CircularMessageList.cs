using UnityEngine;
using System.Collections.Generic;

// ESTRUCTURA DE DATOS - Lista circular de mensajes
public class CircularMessageList
{
    private List<BullyingMessage> messages;
    private int currentIndex = 0;

    public CircularMessageList()
    {
        messages = new List<BullyingMessage>();
    }

    public void AddMessage(BullyingMessage message)
    {
        messages.Add(message);
    }

    public BullyingMessage GetNext()
    {
        if (messages.Count == 0) return null;
        
        BullyingMessage msg = messages[currentIndex];
        currentIndex = (currentIndex + 1) % messages.Count;
        return msg;
    }

    public BullyingMessage GetRandom()
    {
        if (messages.Count == 0) return null;
        int randomIndex = Random.Range(0, messages.Count);
        return messages[randomIndex];
    }

    public int Count => messages.Count;
}