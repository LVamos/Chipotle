using System;
using System.Collections.Generic;

public class MenuParametersDTO
{
	public List<List<string>> Items { get; }
	public string IntroText { get; }
	public string Divider { get; }
	public int SearchIndex { get; }
	public bool WrappingAllowed { get; }
	public string IntroSound { get; }
	public string OutroSound { get; }
	public string SelectionSound { get; }
	public string WrapDownSound { get; }
	public string WrapUpSound { get; }
	public string UpperEdgeSound { get; }
	public string LowerEdgeSound { get; }
	public Action<int> MenuClosed { get; }
	public int DefaultIndex { get; }

	public MenuParametersDTO(
		List<List<string>> items,
		string introText,
		string divider = " ",
		int searchIndex = 0,
		bool wrappingAllowed = true,
		string introSound = null,
		string outroSound = null,
		string selectionSound = null,
		string wrapDownSound = null,
		string wrapUpSound = null,
		string upperEdgeSound = null,
		string lowerEdgeSound = null,
		Action<int> menuClosed = null,
		int defaultIndex = -1)
	{
		Items = items;
		IntroText = introText;
		Divider = divider;
		SearchIndex = searchIndex;
		WrappingAllowed = wrappingAllowed;
		IntroSound = introSound;
		OutroSound = outroSound;
		SelectionSound = selectionSound;
		WrapDownSound = wrapDownSound;
		WrapUpSound = wrapUpSound;
		UpperEdgeSound = upperEdgeSound;
		LowerEdgeSound = lowerEdgeSound;
		MenuClosed = menuClosed;
		DefaultIndex = defaultIndex;
	}
}
