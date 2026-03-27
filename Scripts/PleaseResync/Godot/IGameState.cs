namespace PleaseResync
{
    public interface IGameState
    {
        void Setup();
        void GameLoop(byte[] playerInput);
        byte[] SaveState();
        void LoadState(byte[] stateBuffer);
        void Render();
        byte[] GetLocalInput(int PlayerID, int InputSize);
    }
}
