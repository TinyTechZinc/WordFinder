using WordFinder;

namespace UI.Views;

public partial class FinderRuleView : ContentView
{
	public static readonly BindableProperty RuleCharacterProperty = BindableProperty.Create(
		nameof(RuleCharacter),
		typeof(string),
		typeof(FinderRuleView),
		"",
		BindingMode.TwoWay);
	public string RuleCharacter
	{
		get => (string)GetValue(RuleCharacterProperty);
		set => SetValue(RuleCharacterProperty, value);
	}
	public static readonly BindableProperty RuleNameProperty = BindableProperty.Create(
		nameof(RuleName),
		typeof(string),
		typeof(FinderRuleView),
		"",
		BindingMode.TwoWay);
	public string RuleName
	{
		get => (string)GetValue(RuleNameProperty);
		set => SetValue(RuleNameProperty, value);
	}
	public static readonly BindableProperty RuleNumberProperty = BindableProperty.Create(
		nameof(RuleNumber),
		typeof(string),
		typeof(FinderRuleView),
		"",
		BindingMode.TwoWay);
	public string RuleNumber
	{
		get => (string)GetValue(RuleNumberProperty);
		set => SetValue(RuleNumberProperty, value);
	}
	public static readonly Dictionary<RegexFinder.CharacterRule, string> RuleNameMap = new()
	{
		{ RegexFinder.CharacterRule.AtPosition, "At Position" },
		{ RegexFinder.CharacterRule.NotAtPosition, "Not At Position" },
		{ RegexFinder.CharacterRule.ExactCount, "Exact Count" },
		{ RegexFinder.CharacterRule.MinCount, "Minimum Count" },
		{ RegexFinder.CharacterRule.MaxCount, "Maximum Count" }
	};
	public class RuleDefinition
	{
		public char Character;
		public RegexFinder.CharacterRule RuleType;
		public int Number;
	}
	private RuleDefinition _rule = new();
	public RuleDefinition Rule
	{
		get => _rule;
		set
		{
			_rule = value;
			RuleCharacter = value.Character.ToString();
			RuleNumber = value.Number.ToString();
			RuleName = RuleNameMap[value.RuleType];
		}
	}
	public FinderRuleView()
	{
		InitializeComponent();
		Rule = new();
	}
	public FinderRuleView(RuleDefinition rule, EventHandler editClicked, EventHandler removeClicked) : this()
	{
		EditClicked += editClicked;
		RemoveClicked += removeClicked;
		Rule = rule;
	}

	public event EventHandler? RemoveClicked;
	protected virtual void OnRemoveClicked()
	{
		RemoveClicked?.Invoke(this, EventArgs.Empty);
	}
	public event EventHandler? EditClicked;
	protected virtual void OnEditClicked()
	{
		EditClicked?.Invoke(this, EventArgs.Empty);
	}
	private void ButtonRemove_Clicked(object sender, EventArgs e)
	{
		OnRemoveClicked();
	}
	private void ButtonEdit_Clicked(object sender, EventArgs e)
	{
		OnEditClicked();
	}
}