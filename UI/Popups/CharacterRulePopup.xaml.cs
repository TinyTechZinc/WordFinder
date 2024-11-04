using CommunityToolkit.Maui.Views;
using WordFinder;

namespace UI.Popups;

public partial class CharacterRulePopup : Popup
{
	public static readonly BindableProperty RuleCharacterProperty = BindableProperty.Create(
		nameof(RuleCharacter),
		typeof(string),
		typeof(CharacterRulePopup),
		"",
		BindingMode.TwoWay);
	public string RuleCharacter
	{
		get => (string)GetValue(RuleCharacterProperty);
		set => SetValue(RuleCharacterProperty, value);
	}
	public static readonly BindableProperty RuleTypeIndexProperty = BindableProperty.Create(
		nameof(RuleTypeIndex),
		typeof(int),
		typeof(CharacterRulePopup),
		0,
		BindingMode.TwoWay);
	public int RuleTypeIndex
	{
		get => (int)GetValue(RuleTypeIndexProperty);
		set => SetValue(RuleTypeIndexProperty, value);
	}
	public static readonly BindableProperty RuleNumberProperty = BindableProperty.Create(
		nameof(RuleNumber),
		typeof(string),
		typeof(CharacterRulePopup),
		"",
		BindingMode.TwoWay);
	public string RuleNumber
	{
		get => (string)GetValue(RuleNumberProperty);
		set => SetValue(RuleNumberProperty, value);
	}
	public static readonly List<RegexFinder.CharacterRule> RuleList =
	[
		RegexFinder.CharacterRule.AtPosition,
		RegexFinder.CharacterRule.NotAtPosition,
		//RegexFinder.CharacterRule.Exclude,
		RegexFinder.CharacterRule.ExactCount,
		RegexFinder.CharacterRule.MinCount,
		RegexFinder.CharacterRule.MaxCount
	];
	public CharacterRulePopup()
	{
		InitializeComponent();
		EntryLetter.TextChanged += State_Changed!;
		PickerRule.SelectedIndexChanged += State_Changed!;
		EntryNumber.TextChanged += State_Changed!;
	}
	public CharacterRulePopup(RegexFinder.CharRule rule) : this()
	{
		RuleCharacter = rule.Character.ToString();
		RuleNumber = rule.Number.ToString();
		RuleTypeIndex = RuleList.IndexOf(rule.RuleType);
		ButtonAddRule.Text = "Update Rule";
	}
	private static bool TryCreateRule(string character, int ruleIndex, string number, out RegexFinder.CharRule? rule)
	{
		rule = null;
		if (character.Length != 1 || character != RegexFinder.GetSafeString(character))
		{
			return false;
		}
		if (ruleIndex < 0 || ruleIndex >= RuleList.Count)
		{
			return false;
		}
		if (!int.TryParse(number, out int num) || num < 1)
		{
			return false;
		}
		rule = new()
		{
			Character = character[0],
			Number = num,
			RuleType = RuleList[ruleIndex]
		};
		return true;
	}
	private void State_Changed(object sender, EventArgs e)
	{
		ButtonAddRule.IsEnabled = TryCreateRule(RuleCharacter, RuleTypeIndex, RuleNumber, out var _);
	}
	private async void ButtonAddRule_Clicked(object sender, EventArgs e)
	{
		ButtonAddRule.IsEnabled = false;
		if (TryCreateRule(RuleCharacter, RuleTypeIndex, RuleNumber, out var rule))
		{
			await CloseAsync(rule, new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);
		}
		ButtonAddRule.IsEnabled = true;
	}
}