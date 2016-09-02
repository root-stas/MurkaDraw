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
    /// Структура хранящая образы фигур для рисования в виде списка координат
    /// </summary>
    private struct Shape
    {
        //Можно переписать через массив, но тогда нужно будет написать свою систему добавления\удаления элементов,
        //повышает быстродействие при большем количестве элементов (при отсутствии необходимости использовать всех остальных методов списка)
        /// <summary>
        /// Список координат вершин фигуры
        /// </summary>
        public List<Vector2> Dots;
        /// <summary>
        /// Структура хранящая образы фигур для рисования в виде списка координат
        /// </summary>
        public Shape(Vector2[] _dots)
        {
            Dots = new List<Vector2>(_dots);
        }
    }
    /// <summary>
    /// Список всех фигур для заданий
    /// </summary>
    private List<Shape> Shapes = new List<Shape>();
    /// <summary>
    /// Фигура нарисованная игроком
    /// </summary>
    private Shape Drawed;
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
        Cursor = GameObject.FindGameObjectWithTag("Cursor").transform;
        MenuUI = GameObject.FindGameObjectWithTag("MenuUI");
        GameUI = GameObject.FindGameObjectWithTag("GameUI");
        EditorUI = GameObject.FindGameObjectWithTag("EditorUI");
        DialogUI = GameObject.FindGameObjectWithTag("FailUI");
        TimeTxT = GameObject.FindGameObjectWithTag("TimeTxT").GetComponent<Text>() as Text;
        ScoreTxT = GameObject.FindGameObjectWithTag("ScoreTxT").GetComponent<Text>() as Text;

        //Создание объектов отображающих фигуры
        GameObject LineObj = Instantiate(Resources.Load("Prefabs/ShapeLine", typeof(GameObject))) as GameObject;
        TaskLine = LineObj.GetComponent<LineRenderer>() as LineRenderer;
        LineObj = Instantiate(Resources.Load("Prefabs/ShapeLine", typeof(GameObject))) as GameObject;
        DrawedLine = LineObj.GetComponent<LineRenderer>() as LineRenderer;
        Drawed = new Shape(new Vector2[] { });

        //Добавление примитивных фигур: квадрат, прямоугольник, ромб, равнобедренный треугольник
        Shapes.Add(new Shape(new Vector2[] { new Vector2(-2, -2), new Vector2(2, -2), new Vector2(2, 2), new Vector2(-2, 2) }));
        Shapes.Add(new Shape(new Vector2[] { new Vector2(-3, -2), new Vector2(3, -2), new Vector2(3, 2), new Vector2(-3, 2) }));
        Shapes.Add(new Shape(new Vector2[] { new Vector2(0, -2), new Vector2(-2, 0), new Vector2(0, 2), new Vector2(2, 0) }));
        Shapes.Add(new Shape(new Vector2[] { new Vector2(-3, -2), new Vector2(3, -2), new Vector2(0, 2) }));

        //Инициализация экрана меню и запуск таймера
        MenuBtn(MENU);
        StartCoroutine(OneSecEvent());
    }

    /// <summary>
    /// Выбор и вывод фигуры для задания, очищает прошлый рисунок игрока
    /// </summary>
    private void DrawTask()
    {
        Drawed.Dots.Clear();
        DrawedLine.SetVertexCount(0);
       
        int ID = _TaskID;
        while (ID == _TaskID) ID = Random.Range(0, Shapes.Count);

        TaskLine.SetVertexCount(Shapes[ID].Dots.Count + 1);
        for (int _dot = 0; _dot < Shapes[ID].Dots.Count; _dot++)
            TaskLine.SetPosition(_dot, Shapes[ID].Dots[_dot]);
        TaskLine.SetPosition(Shapes[ID].Dots.Count, Shapes[ID].Dots[0]);
        _TaskID = ID;
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
                    DrawLine(0.02f);

                    //Завершаем рисунок, масштабируем и проверяем на соответствие с заданием
                    if (Input.GetMouseButtonUp(0))
                    {
                        DrawMode = false;
                        Drawed.Dots.Add(CurrentPos);
                        if (Cursor != null) Cursor.gameObject.SetActive(false);
                        if (Drawed.Dots.Count < 3) return;

                        ScaleShape(Shapes[_TaskID], ref Drawed);
                        DrawedLine.SetVertexCount(Drawed.Dots.Count);
                        for (int _dot = 0; _dot < Drawed.Dots.Count; _dot++)
                            DrawedLine.SetPosition(_dot, Drawed.Dots[_dot]);

                        TimeTxT.text = CompareShapes(Shapes[_TaskID], Drawed).ToString();
                        DrawedLine.SetColors(Color.gray, Color.gray);
                    }
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
                    DrawLine(1f);

                    //Завершение рисунка и отображение предложения на сохранение
                    if (Input.GetMouseButtonUp(0))
                    {
                        DrawMode = false;
                        Drawed.Dots.Add(CurrentPos);
                        if (Cursor != null) Cursor.gameObject.SetActive(false);
                        if (Drawed.Dots.Count < 3) return;

                        MenuBtn(EDITOR_DIALOG);
                        DrawedLine.SetColors(Color.gray, Color.gray);
                    }
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
            Drawed.Dots.Clear();
            DrawedLine.SetVertexCount(2);
            if (Cursor != null) Cursor.gameObject.SetActive(true);
            CurrentPos = LastPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward;
            Drawed.Dots.Add(LastPos);
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
            Drawed.Dots.Add(CurrentPos);
            DrawedLine.SetVertexCount(Drawed.Dots.Count + 1);
            DrawedLine.SetPosition(Drawed.Dots.Count - 1, CurrentPos);
            LastPos = CurrentPos;
        }
        DrawedLine.SetPosition(Drawed.Dots.Count, CurrentPos);
    }
    /// <summary>
    /// Определение размера фигуры и ее центра, возвращает массив из 2-х векторов, в первом размер фигуры, во втором координаты центра
    /// </summary>
    /// <param name="_shape">образ фигуры, для которой необходимо провести вычисления</param>
    /// <returns></returns>
    private Vector2[] GetShapeBound(ref Shape _shape)
    {
        Vector2[] result = new Vector2[2];
        float LeftDown, RightDown, LeftUp, RightUp;
        LeftDown = RightDown = _shape.Dots[0].x;
        LeftUp = RightUp = _shape.Dots[0].y;
        result[1] += _shape.Dots[0];

        for (int _dot = 1; _dot < _shape.Dots.Count; _dot++)
        {
            result[1] += _shape.Dots[_dot];
            //Поиск крайних точек
            if (_shape.Dots[_dot].x < LeftDown) LeftDown = _shape.Dots[_dot].x;
            if (_shape.Dots[_dot].x > RightDown) RightDown = _shape.Dots[_dot].x;
            if (_shape.Dots[_dot].y < LeftUp) LeftUp = _shape.Dots[_dot].y;
            if (_shape.Dots[_dot].y > RightUp) RightUp = _shape.Dots[_dot].y;
        }
        //Определение размера
        float _size1 = Mathf.Abs(LeftDown - RightDown);
        float _size2 = Mathf.Abs(LeftUp - RightUp);
        float _size3 = Mathf.Abs(LeftDown - RightDown);
        float _size4 = Mathf.Abs(LeftUp - RightUp);

        result[0] = new Vector2(_size1 > _size2 ? _size1 : _size2, _size3 > _size4 ? _size3 : _size4);
        //Определение центра фигуры
        result[1] /= _shape.Dots.Count;
        return result;
    }

    /// <summary>
    /// Масштабирование фигуры игрока к фигуре по заданию
    /// </summary>
    /// <param name="_original">фигура по заданию, к которой нужно подогнать размеры</param>
    /// <param name="_scalable">фигура игрока, ее размер и положение изменяется к размерам фигуры в _original</param>
    private void ScaleShape(Shape _original, ref Shape _scalable)
    {
        //Определение размеров и центров фигур
        Vector2[] OrigBound = GetShapeBound(ref _original);
        Vector2[] ScaleBound = GetShapeBound(ref _scalable);

        //Определение смещения и масштаба фигуры игрока
        Vector2 MoveCenter = OrigBound[1] - ScaleBound[1];
        Vector2 ScaleFactor = new Vector2(OrigBound[0].x / ScaleBound[0].x, OrigBound[0].y / ScaleBound[0].y);
        //Изменение размера и положения фигуры игрока
        for (int _dot = 0; _dot < _scalable.Dots.Count; _dot++)
            _scalable.Dots[_dot] = new Vector2((_scalable.Dots[_dot].x - ScaleBound[1].x) * ScaleFactor.x, (_scalable.Dots[_dot].y - ScaleBound[1].y) * ScaleFactor.y) + ScaleBound[1] + MoveCenter;
    }

    /// <summary>
    /// Сравнение фигур путем сравнения всех совпавших вершин _task, и если 90% совпали и все остальные вершины _draw не далеко от отрезков _task то возвращает true, иначе false
    /// </summary>
    /// <param name="_task">фигура оригинал</param>
    /// <param name="_draw">фигура которую нужно сравнить с оригиналом</param>
    /// <returns></returns>
    private bool CompareShapes(Shape _task, Shape _draw)
    {
        //Сравнение совпадения со всеми вершинами
        bool[] result = new bool[_task.Dots.Count + 1];
        for (int _dot = 0; _dot < _task.Dots.Count; _dot++)
        {
            for (int _mydot = _draw.Dots.Count - 1; _mydot >= 0; _mydot--)
                if (Mathf.Abs((_task.Dots[_dot] - _draw.Dots[_mydot]).magnitude) < Difficulty * 1.5f)
                {
                    result[_dot] = true;
                    _draw.Dots.RemoveAt(_mydot);
                }
        }

        //Сравнение отступа вершин фигуры _draw от отрезков фигуры _task   
        Vector2 StartLine, EndLine;
        for (int _dot = 0; _dot < _task.Dots.Count; _dot++)
        {
            StartLine = _task.Dots[_dot];
            if (_dot == _task.Dots.Count - 1) EndLine = _task.Dots[0];
            else EndLine = _task.Dots[_dot + 1];

            for (int _mydot = _draw.Dots.Count - 1; _mydot >= 0; _mydot--)
            {
                Vector3 ToDrawDot = _draw.Dots[_mydot] - StartLine;
                Vector3 BtwTaskDot = new Vector3(EndLine.y - StartLine.y, -EndLine.x + StartLine.x, 0);
                Vector3 ProjDot = Vector3.Project(ToDrawDot, BtwTaskDot.normalized);
                Vector2 ProjToDot = new Vector2(_draw.Dots[_mydot].x - ProjDot.x, _draw.Dots[_mydot].y - ProjDot.y) - StartLine;

                if (ProjDot.magnitude < Difficulty)
                    _draw.Dots.RemoveAt(_mydot);
            }
        }

        //Пересчет всех совпавших вершин _task, и если 90% совпали и все остальные вершины _draw не далеко от отрезков _task то возвращает true, иначе false
        int CountDotMatch = 0;
        for (int _dot = 0; _dot < result.Length - 1; _dot++)
            if (result[_dot]) CountDotMatch++;
        if (CountDotMatch > _task.Dots.Count * 0.9f && _draw.Dots.Count == 0) result[result.Length - 1] = true;
        else result[result.Length - 1] = false;

        return result[result.Length - 1];
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
                Drawed.Dots.Clear();
                GameState = EDITOR;
                break;
            case EXIT_GAME:
                Application.Quit();
                break;
            case SAVE_GAME:
                if (GameUI != null) GameUI.SetActive(true);
                if (Drawed.Dots.Count > 3) Shapes.Add(new Shape(Drawed.Dots.ToArray()));
                Drawed.Dots.Clear();
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
