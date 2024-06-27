using System.IO;
namespace PleaseResync
{
    public interface IGameState
    {
        void Setup();
        void GameLoop(PlayerInputs[] playerInput);
        void Serialize(BinaryWriter bw);
        void Deserialize(BinaryReader br);
        PlayerInputs GetLocalInput(int PlayerID);
        int StateFrame();
        //uint StateChecksum();
    }
}
