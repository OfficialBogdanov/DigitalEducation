using System;
using System.Collections;
using System.Collections.Generic;

namespace DigitalEducation.Pages.CreateCustomLesson
{
    public class StepCollection : IEnumerable<LessonStep>
    {
        private readonly List<LessonStep> _steps = new List<LessonStep>();

        public event EventHandler<StepCollectionChangedEventArgs> Changed;

        public int Count => _steps.Count;

        public LessonStep this[int index] => _steps[index];

        public void Add(LessonStep step)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));
            _steps.Add(step);
            OnChanged(new StepCollectionChangedEventArgs(StepChangeType.Add, step, _steps.Count - 1));
        }

        public void Insert(int index, LessonStep step)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));
            _steps.Insert(index, step);
            OnChanged(new StepCollectionChangedEventArgs(StepChangeType.Add, step, index));
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _steps.Count) throw new ArgumentOutOfRangeException(nameof(index));
            var step = _steps[index];
            _steps.RemoveAt(index);
            OnChanged(new StepCollectionChangedEventArgs(StepChangeType.Remove, step, index));
        }

        public bool Remove(LessonStep step)
        {
            int index = _steps.IndexOf(step);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            _steps.Clear();
            OnChanged(new StepCollectionChangedEventArgs(StepChangeType.Reset, null, -1));
        }

        public IEnumerator<LessonStep> GetEnumerator() => _steps.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected virtual void OnChanged(StepCollectionChangedEventArgs e)
        {
            Changed?.Invoke(this, e);
        }
    }

    public enum StepChangeType
    {
        Add,
        Remove,
        Reset
    }

    public class StepCollectionChangedEventArgs : EventArgs
    {
        public StepChangeType ChangeType { get; }
        public LessonStep ChangedStep { get; }
        public int Index { get; }

        public StepCollectionChangedEventArgs(StepChangeType changeType, LessonStep changedStep, int index)
        {
            ChangeType = changeType;
            ChangedStep = changedStep;
            Index = index;
        }
    }
}