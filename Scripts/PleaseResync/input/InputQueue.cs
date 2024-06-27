using System.Collections.Generic;
using System.Diagnostics;

namespace PleaseResync
{
    internal class InputQueue
    {
        public const int QueueSize = 128;
        private uint _frameDelay;
        private GameInput[] _inputs;
        private GameInput[] _lastPredictedInputs;

        public InputQueue(uint inputSize, uint playerCount, uint frameDelay = 0)
        {
            _frameDelay = frameDelay;
            _inputs = new GameInput[QueueSize];
            _lastPredictedInputs = new GameInput[QueueSize];

            for (int i = 0; i < _inputs.Length; i++)
            {
                _inputs[i] = new GameInput(GameInput.NullFrame, inputSize, playerCount);
                _lastPredictedInputs[i] = new GameInput(GameInput.NullFrame, inputSize, playerCount);
            }
        }

        public GameInput GetPredictedInput(int frame)
        {
            return _lastPredictedInputs[frame % QueueSize];
        }

        public uint GetFrameDelay() => _frameDelay;

        public void AddInput(int frame, GameInput input)
        {
            Debug.Assert(frame >= 0);

            frame += (int)_frameDelay;
            _inputs[frame % QueueSize] = new GameInput(input);
            _inputs[frame % QueueSize].Frame = frame;
        }

        public GameInput GetInput(int frame, bool predict = true)
        {
            Debug.Assert(frame >= 0);

            int frameOffset = frame % QueueSize;
            // predict if needed
            if (predict)
            {
                // if the frame is a NullFrame or the frames dont match predict the next frame based off the previous frame.
                if (_inputs[frameOffset].Frame == GameInput.NullFrame ||
                    _inputs[frameOffset].Frame != frame)
                {
                    // predict current frame based off previous frame.
                    var prevFrame = _inputs[PreviousFrame(frameOffset)];
                    _inputs[frameOffset] = new GameInput(prevFrame);
                    _inputs[frameOffset].Frame = GameInput.NullFrame;

                    // add new predicted frame to the queue. when later is proved that the input was right or wrong it will be reset.
                    _lastPredictedInputs[frameOffset] = new GameInput(_inputs[frameOffset]);
                    _lastPredictedInputs[frameOffset].Frame = frame;
                }
            }
            return new GameInput(_inputs[frameOffset]);
        }

        public void ResetPrediction(int frame)
        {
            // when resetting the prediction we just make the frame a null frame.
            int frameOffset = frame % QueueSize;
            _lastPredictedInputs[frameOffset].Frame = GameInput.NullFrame;
        }

        private int PreviousFrame(int offset) => (((offset) == 0) ? (QueueSize - 1) : ((offset) - 1));
    }
}