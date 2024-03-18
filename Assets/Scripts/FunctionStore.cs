using System;
using System.Threading.Tasks;
using Networking.openIAExtension;
using Networking.openIAExtension.Commands;
using Snapshots;
using UnityEngine;
using Random = System.Random;

public class FunctionStore : MonoBehaviour
{
    public static FunctionStore Instance { get; private set; }
    
    [SerializeField]
    private bool offline;
    
    [SerializeField]
    private OpenIaWebSocketClient openIaWebSocketClient;

    private readonly Random _random = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    public Func<CreateSnapshot, Task> GetSnapshotRegistrationFunction()
    {
        if (offline)
        {
            return c =>
            {
                var buffer = new byte[8];
                _random.NextBytes(buffer);
                var id = BitConverter.ToUInt64(buffer);
                SnapshotManager.Instance.CreateSnapshot(id, c.Position, c.Rotation);
                return Task.CompletedTask;
            };
        }
        else
        {
            return async c => await openIaWebSocketClient.Send(c);
        }
    }
}