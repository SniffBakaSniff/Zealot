namespace Zealot.Shared
{
    public class DuplicateWarningEscalationRuleException(int warningCount) : Exception($"A rule already exists for {warningCount} warnings.");
    public class MaximumWarningEscalationRuleException(int maxAllowed) : Exception($"Maximum warning escalation rules reached ({maxAllowed}).")
    {
        public int MaxAllowed { get; } = maxAllowed;
    }
}