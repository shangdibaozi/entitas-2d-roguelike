﻿using Entitas;
using System.Collections;
using UnityEngine;

public class SmoothMoveSystem : IExecuteSystem, ISetPool
{
    CoroutineHandler coroutines = new CoroutineHandler();
    Group smoothMoveGroup;

    void ISetPool.SetPool(Pool pool)
    {
        smoothMoveGroup = pool.GetGroup(Matcher.AllOf(Matcher.View,
                                                      Matcher.Position,
                                                      Matcher.SmoothMove));
    }

    void IExecuteSystem.Execute()
    {
        coroutines.Update();

        var entities = smoothMoveGroup.GetEntities();
        foreach (var e in entities)
        {
            if (e.isSmoothMoveInProgress)
            {
                // already being handled, ignore
                continue;
            }

            var position = e.position;
            var viewPosition = e.view.gameObject.transform.position;
            if (position.x != viewPosition.x || position.y != viewPosition.y)
            {
                coroutines.Add(SmoothMovement(e));
            }
        }
    }

    IEnumerator SmoothMovement(Entity entity)
    {
        entity.isSmoothMoveInProgress = true;
        var gameObject = entity.view.gameObject;
        var transform = gameObject.transform;
        var rigidBody2d = gameObject.GetComponent<Rigidbody2D>();
        var end = new Vector3(entity.position.x, entity.position.y, 0f);
        var inverseMoveTime = 1f / entity.smoothMove.moveTime;
        var sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector2 newPostion = Vector2.MoveTowards(rigidBody2d.position, end, inverseMoveTime * Time.deltaTime);
            rigidBody2d.MovePosition(newPostion);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }
        entity.isSmoothMoveInProgress = false;
    }
}
