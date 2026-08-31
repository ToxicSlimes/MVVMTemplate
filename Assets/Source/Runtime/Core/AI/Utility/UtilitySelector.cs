using System;

namespace Core.AI.Utility
{
    public static class UtilitySelector
    {
        public static TGoal Choose<TGoal>(
            TGoal[] goals,
            TGoal current,
            float inertiaBonus,
            Func<TGoal, float> score)
            where TGoal : IEquatable<TGoal>
        {
            if (goals == null || goals.Length == 0)
            {
                throw new ArgumentException("Goals cannot be empty.", nameof(goals));
            }

            if (score == null)
            {
                throw new ArgumentNullException(nameof(score));
            }

            var best = goals[0];
            var bestScore = float.NegativeInfinity;

            for (var i = 0; i < goals.Length; i++)
            {
                var goal = goals[i];
                var value = score(goal);

                if (goal.Equals(current))
                {
                    value += inertiaBonus;
                }

                if (value <= bestScore)
                {
                    continue;
                }

                bestScore = value;
                best = goal;
            }

            return best;
        }
    }
}
