using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Структура хранящая образы фигур для рисования в виде списка координат
/// </summary>
public class TShape
{
    //Можно переписать через массив, но тогда нужно будет написать свою систему добавления\удаления элементов,
    //повышает быстродействие при большем количестве элементов (при отсутствии необходимости использовать всех остальных методов списка)
    /// <summary>
    /// Список координат вершин фигуры
    /// </summary>
    private List<Vector2> Dots;

    /// <summary>
    /// Структура хранящая образы фигур для рисования в виде списка координат
    /// </summary>
    public TShape(Vector2[] _dots)
    {
        Dots = new List<Vector2>(_dots);
    }

    public void Clear()
    {
        Dots.Clear();
    }

    public int Count()
    {
        return Dots.Count;
    }

    public void Add(Vector2 _value)
    {
        Dots.Add(_value);
    }

    public void Draw(ref LineRenderer _plot, bool _closed = false)
    {
        _plot.SetVertexCount(Dots.Count + (_closed ? 1 : 0));
        for (int _dot = 0; _dot < Dots.Count; _dot++)
            _plot.SetPosition(_dot, Dots[_dot]);
        if (_closed) _plot.SetPosition(Dots.Count, Dots[0]);
    }

    public Vector2[] ToArray()
    {
        return Dots.ToArray();
    }

    /// <summary>
    /// Определение размера фигуры и ее центра, возвращает массив из 2-х векторов, в первом размер фигуры, во втором координаты центра
    /// </summary>
    /// <param name="_shape">образ фигуры, для которой необходимо провести вычисления</param>
    /// <returns></returns>
    public Vector2[] GetShapeBound()
    {
        Vector2[] result = new Vector2[2];
        float LeftDown, RightDown, LeftUp, RightUp;
        LeftDown = RightDown = Dots[0].x;
        LeftUp = RightUp = Dots[0].y;
        result[1] += Dots[0];

        for (int _dot = 1; _dot < Dots.Count; _dot++)
        {
            result[1] += Dots[_dot];
            //Поиск крайних точек
            if (Dots[_dot].x < LeftDown) LeftDown = Dots[_dot].x;
            if (Dots[_dot].x > RightDown) RightDown = Dots[_dot].x;
            if (Dots[_dot].y < LeftUp) LeftUp = Dots[_dot].y;
            if (Dots[_dot].y > RightUp) RightUp = Dots[_dot].y;
        }
        //Определение размера
        float _size1 = Mathf.Abs(LeftDown - RightDown);
        float _size2 = Mathf.Abs(LeftUp - RightUp);
        float _size3 = Mathf.Abs(LeftDown - RightDown);
        float _size4 = Mathf.Abs(LeftUp - RightUp);

        result[0] = new Vector2(_size1 > _size2 ? _size1 : _size2, _size3 > _size4 ? _size3 : _size4);
        //Определение центра фигуры
        result[1] /= Dots.Count;
        return result;
    }

    /// <summary>
    /// Масштабирование фигуры игрока к фигуре по заданию
    /// </summary>
    /// <param name="_original">фигура по заданию, к которой нужно подогнать размеры</param>
    public void ScaleShape(TShape _original)
    {
        //Определение размеров и центров фигур
        Vector2[] OrigBound = _original.GetShapeBound();
        Vector2[] ScaleBound = GetShapeBound();

        //Определение смещения и масштаба фигуры игрока
        Vector2 MoveCenter = OrigBound[1] - ScaleBound[1];
        Vector2 ScaleFactor = new Vector2(OrigBound[0].x / ScaleBound[0].x, OrigBound[0].y / ScaleBound[0].y);
        //Изменение размера и положения фигуры игрока
        for (int _dot = 0; _dot < Dots.Count; _dot++)
            Dots[_dot] = new Vector2((Dots[_dot].x - ScaleBound[1].x) * ScaleFactor.x, (Dots[_dot].y - ScaleBound[1].y) * ScaleFactor.y) + ScaleBound[1] + MoveCenter;
    }

    /// <summary>
    /// Сравнение фигур путем сравнения всех совпавших вершин _task, и если 90% совпали и все остальные вершины _draw не далеко от отрезков _task то возвращает true, иначе false
    /// </summary>
    /// <param name="_task">фигура оригинал</param>
    /// <param name="_diff">фигура которую нужно сравнить с оригиналом</param>
    /// <returns></returns>
    public bool CompareShapes(TShape _task, float _diff = 0.2f)
    {
        //Сравнение совпадения со всеми вершинами
        bool[] result = new bool[_task.Dots.Count + 1];
        bool _markDot = false;
        for (int _mydot = Dots.Count - 1; _mydot >= 0; _mydot--)
        {
            _markDot = false;
            for (int _dot = 0; _dot < _task.Dots.Count; _dot++)
                if (Mathf.Abs((_task.Dots[_dot] - Dots[_mydot]).magnitude) < _diff * 1.5f)
                {
                    result[_dot] = true;
                    _markDot = true;
                }
            if (_markDot) Dots.RemoveAt(_mydot);
        }

        //Сравнение отступа вершин фигуры _draw от отрезков фигуры _task   
        Vector2 StartLine, EndLine;
        for (int _dot = 0; _dot < _task.Dots.Count; _dot++)
        {
            StartLine = _task.Dots[_dot];
            if (_dot == _task.Dots.Count - 1) EndLine = _task.Dots[0];
            else EndLine = _task.Dots[_dot + 1];

            for (int _mydot = Dots.Count - 1; _mydot >= 0; _mydot--)
            {
                Vector3 ToDrawDot = Dots[_mydot] - StartLine;
                Vector3 BtwTaskDot = new Vector3(EndLine.y - StartLine.y, -EndLine.x + StartLine.x, 0);
                Vector3 ProjDot = Vector3.Project(ToDrawDot, BtwTaskDot.normalized);
                Vector2 ProjToDot = new Vector2(Dots[_mydot].x - ProjDot.x, Dots[_mydot].y - ProjDot.y) - StartLine;

                if (ProjDot.magnitude < _diff)
                    Dots.RemoveAt(_mydot);
            }
        }

        //Пересчет всех совпавших вершин _task, и если 90% совпали и все остальные вершины _draw не далеко от отрезков _task то возвращает true, иначе false
        int CountDotMatch = 0;
        for (int _dot = 0; _dot < result.Length - 1; _dot++)
            if (result[_dot]) CountDotMatch++;
        if (CountDotMatch == _task.Dots.Count && Dots.Count == 0) result[result.Length - 1] = true;
        else result[result.Length - 1] = false;
        Debug.Log(CountDotMatch + " " + Dots.Count);

        return result[result.Length - 1];
    }
}