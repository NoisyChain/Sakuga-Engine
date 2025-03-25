using System.IO;
namespace PleaseResync
{
    public interface IGameState
    {
        void Setup();
        void GameLoop(byte[] playerInput);
        void SaveState(BinaryWriter bw);
        void LoadState(BinaryReader br);
        void Render();
        byte[] GetLocalInput(int PlayerID, int InputSize);
    }
}
