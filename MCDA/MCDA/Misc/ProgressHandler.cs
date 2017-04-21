using System;

namespace MCDA.Misc
{
    /// <summary>
    /// Represents the progress for a given task.
    /// </summary>
    public sealed class ProgressHandler
    {
        private double _progress;
        private double _progressBeforeChild;
        private string _text;
        private ProgressHandler _currentChildHandler;

        /// <summary>
        /// Updates the progress.
        /// </summary>
        /// <param name="progress">Value between 0 and 100.</param>
        /// <param name="text">Description of the current task.</param>
        public delegate void ProgressEventHandler(int progress, string text);

        /// <summary>
        /// Event if progress was made on the given task.
        /// </summary>
        public event ProgressEventHandler ProgressEvent;

        /// <summary>
        /// Called to update the progress.
        /// </summary>
        /// <param name="progress"> Value between 0 and 100.</param>
        /// <param name="text">Description of the current task.</param>
        public void OnProgress(int progress, string text)
        {
            _progress = progress;
            _text = text;
            ProgressEvent?.Invoke(progress, text);
        }

        /// <summary>
        /// Calculates the progress based on the given from and to params.
        /// Caller must ensure that the to value is > 0.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to">Must be > 0.</param>
        /// <param name="text">Description of the current task.</param>
        public void OnProgress(int from, int to, string text)
        {
            int progress = (int) (from/(float) to*100);
            _progress = progress;
            _text = text;
            ProgressEvent?.Invoke(progress, text);
        }

        /// <summary>
        /// The progress, a value [0,100].
        /// </summary>
        public double Progress => _progress;

        /// <summary>
        /// The description of the current task
        /// </summary>
        public string Text => _text;

        /// <summary>
        /// Provides a child handler which reports a given fraction of the overall progress.
        /// </summary>
        /// <param name="partFromOverallTaskInPercent">Percent of the progress from the upper/overall task.</param>
        /// <returns></returns>
        public ProgressHandler ProvideChildProgressHandler(int partFromOverallTaskInPercent)
        {
            _currentChildHandler?.OnProgress(100, String.Empty);

            ProgressHandler newChild = new ProgressHandler();
            newChild.ProgressEvent += (progress, text) =>
            {
                int overallProgress = (int)(_progressBeforeChild + partFromOverallTaskInPercent * progress / 100d);
                OnProgress(overallProgress, text);
            };

            _progressBeforeChild = Progress;
            _currentChildHandler = newChild;
            return newChild;
        }
    }
}
