using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MainSrc : MonoBehaviour
{
    /// <summary>
    /// Начальное время на выполнения задания
    /// </summary>
    public int StartTime = 60;
    /// <summary>
    /// Шаг, с которым будет уменьшаться начальное время на каждом новом уровне
    /// </summary>
    public int TimeStep = 5;
    /// <summary>
    /// Так сказать чувствительность при сравнении рисунка с заданием, чем больше тем больше отклонений может быть от задания (был квадрат, нарисовали круг, а игра посчитает что все нормально)
    /// </summary>
    [Range(0.2f, 1f)]
    public float Difficulty = 0.5f;

    /// <summary>
    /// Объект указателя с эффектом хвоста кометы
    /// </summary>
    private Transform Cursor;

    /// Элементы игрового интерфейса и игровых экранов 
    /// <summary>
    /// Текстовое поле отображающие остаток времени на выполнение задания
    /// </summary>
    private Text TimeTxT;
    /// <summary>
    /// Текстовое поле отображающие счет игрока
    /// </summary>
    private Text ScoreTxT;
    /// <summary>
    /// Экран меню, кнопки новой игры, редактора и выхода
    /// </summary>
    private GameObject MenuUI;
    /// <summary>
    /// Игровой экран, кнопка выхода в меню и время раунда
    /// </summary>
    private GameObject GameUI;
    /// <summary>
    /// Экран диалога редактора, сохранение фигуры или выход в меню
    /// </summary>
    private GameObject EditorUI;
    /// <summary>
    /// Окно проигрыша со счетом и перезапуском уровня
    /// </summary>
    private GameObject DialogUI;
    /// <summary>
    /// Текущий счет игры
    /// </summary>
    private int Score = 0;
    /// <summary>
    /// Остаток времени на выполнения задания
    /// </summary>
    private int GameTime;
    /// <summary>
    /// Объект отображающий задание для повторения
    /// </summary>
    private LineRenderer TaskLine;
    /// <summary>
    /// Объект отображающий рисунок игрока
    /// </summary>
    private LineRenderer DrawedLine;

    //Константы, определяющие состояние игры, для удобства чтения кода
    private const byte MENU = 0;
    private const byte GAME = 1;
    private const byte GAME_DIALOG = 2;
    private const byte EDITOR = 3;
    private const byte EDITOR_DIALOG = 4;
    /// <summary>
    /// Состояние игры, принимает значения из объявленных констант MENU, GAME, GAME_DIALOG, EDITOR, EDITOR_DIALOG
    /// </summary>
    private byte GameState = MENU;
    //Дополнительные состояния игры, для функции MenuBtn
    private const byte EXIT_GAME = 5;
    private const byte SAVE_GAME = 6;

    /// <summary>
    /// Список всех фигур для заданий
    /// </summary>
    private List<TShape> Shapes = new List<TShape>();
    /// <summary>
    /// Фигура нарисованная игроком
    /// </summary>
    private TShape Drawed;
    /// <summary>
    /// Текущий номер фигуры для задания
    /// </summary>
    private int _TaskID = -1;
    /// <summary>
    /// Последняя добавленная вершина в фигуру игрока
    /// </summary>
    private Vector3 LastPos;
    /// <summary>
    /// Текущие координаты указателя, возможный кандидат на следующую вершину в фигуру игрока
    /// </summary>
    private Vector3 CurrentPos;
    /// <summary>
    /// Режим рисования, true - когда игрок рисует (зажата ЛКМ или касание с удержанием на сенсорном экране)
    /// </summary>
    private bool DrawMode = false;

    /// <summary>
    /// Инициализация игры, сбор всех необходимых компонентов из сцены и создание новых
    /// </summary>
    void Start()
    {
        //Сбор всех необходимых компонентов из сцены
        Cursor = GameObject.Find("CursorFX").transform;
        MenuUI = GameObject.Find("Menu");
        GameUI = GameObject.Find("Game");
        EditorUI = GameObject.Find("Editor");
        DialogUI = GameObject.Find("Fail");
        TimeTxT = GameObject.Find("TimeTxt").GetComponent<Text>() as Text;
        ScoreTxT = GameObject.Find("ScoreTxt").GetComponent<Text>() as Text;

        //Создание объектов отображающих фигуры
        GameObject LineObj = Instantiate(Resources.Load("Prefabs/ShapeLine", typeof(GameObject))) as GameObject;
        TaskLine = LineObj.GetComponent<LineRenderer>() as LineRenderer;
        LineObj = Instantiate(Resources.Load("Prefabs/ShapeLine", typeof(GameObject))) as GameObject;
        DrawedLine = LineObj.GetComponent<LineRenderer>() as LineRenderer;
        Drawed = new TShape(new Vector2[] { });
        
        //Добавление примитивных фигур: квадрат, прямоугольник, ромб, равнобедренный треугольник
        Shapes.Add(new TShape(new Vector2[] { new Vector2(-2, -2), new Vector2(2, -2), new Vector2(2, 2), new Vector2(-2, 2) }));
        Shapes.Add(new TShape(new Vector2[] { new Vector2(-3, -2), new Vector2(3, -2), new Vector2(3, 2), new Vector2(-3, 2) }));
        Shapes.Add(new TShape(new Vector2[] { new Vector2(0, -2), new Vector2(-2, 0), new Vector2(0, 2), new Vector2(2, 0) }));
        Shapes.Add(new TShape(new Vector2[] { new Vector2(-3, -2), new Vector2(3, -2), new Vector2(0, 2) }));

        //Инициализация экрана меню и запуск таймера
        MenuBtn(MENU);
        StartCoroutine(OneSecEvent());
    }

    /// <summary>
    /// Выбор и вывод фигуры для задания, очищает прошлый рисунок игрока
    /// </summary>
    private void DrawTask()
    {
        int ID = _TaskID;
        while (ID == _TaskID) ID = Random.Range(0, Shapes.Count);
        _TaskID = ID;

        Shapes[ID].Draw(ref TaskLine, true);
    }

    /// <summary>
    /// Таймер в 1с, обновляет время до конца игры и определяет провал задания
    /// </summary>
    /// <returns></returns>
    IEnumerator OneSecEvent()
    {
        while (true)
        {
            if (GameState == GAME)
            {
                GameTime--;
                TimeTxT.text = GameTime.ToString();
                if (GameTime < 0) MenuBtn(GAME_DIALOG);
            }
            yield return new WaitForSeconds(1);
        }
    }

    //Обновление экрана игры и обработка ввода
    void Update()
    {
        switch (GameState)
        {
            case GAME:
                if (Input.GetMouseButtonDown(0)) DrawLine(-1f);
                if (DrawMode)
                {
                    //Завершаем рисунок, масштабируем и проверяем на соответствие с заданием
                    if (Input.GetMouseButtonUp(0))
                    {
                        DrawMode = false;
                        Drawed.Add(CurrentPos);
                        if (Cursor != null) Cursor.gameObject.SetActive(false);
                        if (Drawed.Count() < 3) return;

                        Drawed.ScaleShape(Shapes[_TaskID]);
                        Drawed.Draw(ref DrawedLine);

                        TimeTxT.text = Drawed.CompareShapes(Shapes[_TaskID], Difficulty).ToString();
                        DrawedLine.SetColors(Color.gray, Color.gray);
                    } else DrawLine(0.02f);
                }

                //Переход к следующему заданию
                if (TimeTxT.text == "True")
                {
                    Score++;
                    GameTime = StartTime - Score * TimeStep;
                    TimeTxT.text = GameTime.ToString();
                    DrawTask();
                }
                break;

            case EDITOR:
                if (Input.GetMouseButtonDown(0)) DrawLine(-1f);
                if (DrawMode)
                {
                    TimeTxT.text = "Точек: " + Drawed.Count().ToString();
                    //Завершение рисунка и отображение предложения на сохранение
                    if (Input.GetMouseButtonUp(0))
                    {
                        DrawMode = false;
                        Drawed.Add(CurrentPos);
                        if (Cursor != null) Cursor.gameObject.SetActive(false);
                        if (Drawed.Count() < 3) return;

                        MenuBtn(EDITOR_DIALOG);
                        DrawedLine.SetColors(Color.gray, Color.gray);
                    } else DrawLine(1f);
                }
                break;
        }
    }

    /// <summary>
    /// Рисование игроком фигуры, на вход принимает чувствительность к добавлению точек
    /// </summary>
    /// <param name="_sensitivity">чувствительность к добавлению точек, если меньше 0, то инициирует фигуру на экране и отображает курсор</param>
    private void DrawLine(float _sensitivity)
    {
        //Инициирует фигуру на экране и отображает курсор
        if (_sensitivity < 0f)
        {
            DrawedLine.SetColors(Color.green, Color.green);
            DrawMode = true;
            Drawed.Clear();
            DrawedLine.SetVertexCount(2);
            if (Cursor != null) Cursor.gameObject.SetActive(true);
            CurrentPos = LastPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward;
            Drawed.Add(LastPos);
            DrawedLine.SetPosition(0, LastPos);
            DrawedLine.SetPosition(1, LastPos);
            return;
        }

        //Рисуем курсор
        CurrentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward;
        if (Cursor != null) Cursor.position = CurrentPos;

        //Добавление вершины
        if (Mathf.Abs((CurrentPos - LastPos).sqrMagnitude) > _sensitivity)
        {
            Drawed.Add(CurrentPos);
            DrawedLine.SetVertexCount(Drawed.Count() + 1);
            DrawedLine.SetPosition(Drawed.Count() - 1, CurrentPos);
            LastPos = CurrentPos;
        }
        DrawedLine.SetPosition(Drawed.Count(), CurrentPos);
    }

    /// <summary>
    /// Изменение состояния игры в зависимости от значения параметра Value: MENU, GAME, GAME_DIALOG, EDITOR, EDITOR_DIALOG, EXIT_GAME, SAVE_GAME
    /// </summary>
    /// <param name="Value">задает новое состояние игре, принимает следующие значения констант: MENU, GAME, GAME_DIALOG, EDITOR, EDITOR_DIALOG, EXIT_GAME, SAVE_GAME</param>
    public void MenuBtn(int Value)
    {
        //Обнуление состояния
        if (EditorUI != null) EditorUI.SetActive(false);
        if (DialogUI != null) DialogUI.SetActive(false);
        if (GameUI != null) GameUI.SetActive(false);
        if (MenuUI != null) MenuUI.SetActive(false);
        if (Cursor != null) Cursor.gameObject.SetActive(false);

        //Задание нового состояния
        switch (Value)
        {
            case MENU:
                if (MenuUI != null) MenuUI.SetActive(true);
                GameState = MENU;
                DrawedLine.SetVertexCount(0);
                TaskLine.SetVertexCount(0);
                break;
            case GAME:
                if (GameUI != null) GameUI.SetActive(true);
                GameTime = StartTime;
                GameState = GAME;
                Score = 0;
                TimeTxT.text = GameTime.ToString();
                DrawTask();
                break;
            case EDITOR:
                if (GameUI != null) GameUI.SetActive(true);
                TaskLine.SetVertexCount(0);
                DrawedLine.SetVertexCount(0);
                Drawed.Clear();
                GameState = EDITOR;
                break;
            case EXIT_GAME:
                Application.Quit();
                break;
            case SAVE_GAME:
                if (GameUI != null) GameUI.SetActive(true);
                if (Drawed.Count() > 3) Shapes.Add(new TShape(Drawed.ToArray()));
                Drawed.Clear();
                DrawedLine.SetVertexCount(0);
                GameState = EDITOR;
                break;
            case GAME_DIALOG:
                if (DialogUI != null) DialogUI.SetActive(true);
                ScoreTxT.text = Score.ToString();
                GameState = GAME_DIALOG;
                break;
            case EDITOR_DIALOG:
                if (EditorUI != null) EditorUI.SetActive(true);
                GameState = EDITOR_DIALOG;
                break;
        }
    }
}