using UnityEngine;

public class AndroidDialog : MonoBehaviour
{
    [Header("Настройки текста окна")]
    [Tooltip("Заголовок диалогового окна")]
    [SerializeField] private string dialogTitle = "Important warning";
    
    [Tooltip("Основной текст (сообщение) внутри окна")]
    [TextArea(3, 10)] // Делает поле в инспекторе удобным для длинного текста
    [SerializeField] private string dialogMessage = "This game is at an early stage of development. Please keep in mind that the game may behave strangely at certain points.";
    
    [Tooltip("Текст на кнопке подтверждения")]
    [SerializeField] private string acceptButtonText = "Accept";

    private const string FirstRunKey = "IsFirstRun";

    void Start()
    {
        // Проверяем первый запуск
        if (PlayerPrefs.GetInt(FirstRunKey, 0) == 0)
        {
            // Запускаем окно в нативном потоке Android (UI Thread)
            #if UNITY_ANDROID && !UNITY_EDITOR
            ShowNativeDialog(dialogTitle, dialogMessage, acceptButtonText);
            #else
            Debug.Log($"[Редактор] Окно: {dialogTitle} | Текст: {dialogMessage} | Кнопка: {acceptButtonText}");
            OnDialogClosed(); // Для тестов в редакторе Unity
            #endif
        }
        else
        {
            // Если это НЕ первый запуск, сразу выключаем объект
            gameObject.SetActive(false);
        }
    }

    private void ShowNativeDialog(string title, string message, string buttonText)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            AndroidJavaObject dialogBuilder = new AndroidJavaObject("android.app.AlertDialog$Builder", currentActivity);
            
            dialogBuilder.Call<AndroidJavaObject>("setTitle", title);
            dialogBuilder.Call<AndroidJavaObject>("setMessage", message);
            dialogBuilder.Call<AndroidJavaObject>("setCancelable", false); // Запрещаем закрывать нажатием мимо окна

            // Устанавливаем кнопку «Принять» с вашим текстом
            dialogBuilder.Call<AndroidJavaObject>("setPositiveButton", buttonText, new AndroidDialogClickListener(this));

            AndroidJavaObject dialog = dialogBuilder.Call<AndroidJavaObject>("create");
            dialog.Call("show");
        }));
    }

    public void OnDialogClosed()
    {
        // Сохраняем флаг первого запуска
        PlayerPrefs.SetInt(FirstRunKey, 1);
        PlayerPrefs.Save();
        Debug.Log("Игрок нажал кнопку 'Принять'. Отключаем объект.");

        // Отключаем объект на сцене
        gameObject.SetActive(false); 
    }
}

// Вспомогательный класс для обработки нажатия на кнопку "Принять"
public class AndroidDialogClickListener : AndroidJavaProxy
{
    private AndroidDialog mainScript;

    public AndroidDialogClickListener(AndroidDialog script) : base("android.content.DialogInterface$OnClickListener")
    {
        mainScript = script;
    }

    public void onClick(AndroidJavaObject dialog, int id)
    {
        mainScript.OnDialogClosed();
        dialog.Call("dismiss");
    }
}
