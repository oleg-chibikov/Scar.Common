using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Scar.Common.WPF.Controls;

sealed class TextEditorWrapper
{
    static readonly Type TextEditorType = Type.GetType("System.Windows.Documents.TextEditor, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35") ??
                                          throw new InvalidOperationException("TextEditorType is null");

    static readonly PropertyInfo IsReadOnlyProp =
        TextEditorType.GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("IsReadOnlyProp is null");

    static readonly PropertyInfo TextViewProp =
        TextEditorType.GetProperty("TextView", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("TextViewProp is null");

    static readonly MethodInfo RegisterMethod = TextEditorType.GetMethod(
                                                    "RegisterCommandHandlers",
                                                    BindingFlags.Static | BindingFlags.NonPublic,
                                                    null,
                                                    new[]
                                                    {
                                                        typeof(Type),
                                                        typeof(bool),
                                                        typeof(bool),
                                                        typeof(bool)
                                                    },
                                                    null) ??
                                                throw new InvalidOperationException("RegisterMethod is null");

    static readonly Type TextContainerType = Type.GetType("System.Windows.Documents.ITextContainer, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35") ??
                                             throw new InvalidOperationException("TextContainerType is null");

    static readonly PropertyInfo TextContainerTextViewProp = TextContainerType.GetProperty("TextView") ?? throw new InvalidOperationException("TextContainerTextViewProp is null");
    static readonly PropertyInfo TextContainerProp = typeof(TextBlock).GetProperty("TextContainer", BindingFlags.Instance | BindingFlags.NonPublic) ??
                                                     throw new InvalidOperationException("TextContainerProp is null");

    readonly object _editor;

    public TextEditorWrapper(object textContainer, FrameworkElement uiScope, bool isUndoEnabled)
    {
        _ = uiScope ?? throw new ArgumentNullException(nameof(uiScope));
        _ = textContainer ?? throw new ArgumentNullException(nameof(textContainer));

        _editor = Activator.CreateInstance(
                      TextEditorType,
                      BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                      null,
                      new[]
                      {
                          textContainer,
                          uiScope,
                          isUndoEnabled
                      },
                      null) ??
                  throw new InvalidOperationException("Cannot create instance of TextEditorType");
    }

    public static void RegisterCommandHandlers(Type controlType, bool acceptsRichContent, bool readOnly, bool registerEventListeners)
    {
        _ = controlType ?? throw new ArgumentNullException(nameof(controlType));
        RegisterMethod.Invoke(
            null,
            new object[]
            {
                controlType,
                acceptsRichContent,
                readOnly,
                registerEventListeners
            });
    }

    public static TextEditorWrapper CreateFor(TextBlock tb)
    {
        _ = tb ?? throw new ArgumentNullException(nameof(tb));
        var textContainer = TextContainerProp.GetValue(tb) ?? throw new InvalidOperationException("Cannot get value of TextContainerProp");

        var editor = new TextEditorWrapper(textContainer, tb, false);
        IsReadOnlyProp.SetValue(editor._editor, true);
        TextViewProp.SetValue(editor._editor, TextContainerTextViewProp.GetValue(textContainer));

        return editor;
    }
}
