using Coil.Connections;
using UnityEngine;

namespace ButterBoard
{
    public class GameManager : SingletonBehaviour<GameManager>
    {
        public ConnectionManager ConnectionManager { get; } = new ConnectionManager();
    }
}