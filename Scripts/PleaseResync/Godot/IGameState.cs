using System.IO;
namespace PleaseResync
{
    public interface IGameState
    {
        void Setup();
        void GameLoop(byte[] playerInput);
        void Serialize(BinaryWriter bw);
        void Deserialize(BinaryReader br);
        byte[] GetLocalInput(int PlayerID, int InputSize);
    }
}
