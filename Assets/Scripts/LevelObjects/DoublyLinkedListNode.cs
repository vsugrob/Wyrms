using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DoublyLinkedListNode : MonoBehaviour {
	[SerializeField, HideInInspector]
	private DoublyLinkedListNode nextNode;
	[SerializeField, HideInInspector]
	private DoublyLinkedListNode prevNode;

	[ExposeProperty]
	public DoublyLinkedListNode Next {
		get { return	nextNode; }
		set {
			if ( value == this || value == nextNode )
				return;

			if ( value != null ) {
				value.prevNode = this;

#if UNITY_EDITOR
				if ( !Application.isPlaying )
					UnityEditor.EditorUtility.SetDirty ( value );
#endif
			}

			if ( nextNode != null ) {
				nextNode.prevNode = null;

#if UNITY_EDITOR
				if ( !Application.isPlaying )
					UnityEditor.EditorUtility.SetDirty ( nextNode );
#endif
			}

			nextNode = value;
		}
	}
	[ExposeProperty]
	public DoublyLinkedListNode Prev {
		get { return	prevNode; }
		set {
			if ( value == this || value == prevNode )
				return;

			if ( value != null ) {
				value.nextNode = this;

#if UNITY_EDITOR
				if ( !Application.isPlaying )
					UnityEditor.EditorUtility.SetDirty ( value );
#endif
			}

			if ( prevNode != null ) {
				prevNode.nextNode = null;

#if UNITY_EDITOR
				if ( !Application.isPlaying )
					UnityEditor.EditorUtility.SetDirty ( prevNode );
#endif
			}

			prevNode = value;
		}
	}

	public Vector2 Position { get { return	( Vector2 ) transform.position; } }
	public Vector2 NextPosition { get { return	nextNode != null ? nextNode.Position : this.Position; } }
	public Vector2 PrevPosition { get { return	prevNode != null ? prevNode.Position : this.Position; } }
	public float NextEdgeLength {
		get {
			if ( nextNode != null )
				return	Vector2.Distance ( nextNode.Position, this.Position );
			else
				return	0;
		}
	}
	public float PrevEdgeLength {
		get {
			if ( prevNode != null )
				return	Vector2.Distance ( prevNode.Position, this.Position );
			else
				return	0;
		}
	}

	public List <DoublyLinkedListNode> List {
		get {
			var list = new List <DoublyLinkedListNode> ();
			list.Add ( this );

			var node = this;

			while ( node.prevNode != null && node.prevNode != this ) {
				node = node.prevNode;
				list.Insert ( 0, node );
			}

			if ( node.prevNode == this ) {
				// It's a circular list.

				return	list;
			}

			node = this;

			while ( ( node = node.nextNode ) != null ) {
				list.Add ( node );
			}

			return	list;
		}
	}

	public DoublyLinkedListNode First {
		get {
			var node = this;

			while ( node.prevNode != null && node.prevNode != this ) {
				node = node.prevNode;
			}

			if ( node.prevNode == this ) {
				// It's a circular list.

				return	this;
			} else
				return	node;
		}
	}

	public DoublyLinkedListNode GetClosestEdge ( Vector2 origin, out Vector2 closestPos ) {
		var firstNode = this.First;
		var node = firstNode;
		var closestEdgeStart = node;
		closestPos = closestEdgeStart.Position;
		float minDistanceSq = float.PositiveInfinity;

		do {
			var nodePos = node.Position;

			// Distance to the start node of the edge.
			var vFromNode = origin - nodePos;
			float distFromNodeSq = vFromNode.sqrMagnitude;

			if ( distFromNodeSq < minDistanceSq ) {
				minDistanceSq = distFromNodeSq;
				closestEdgeStart = node;
				closestPos = nodePos;
			}

			var next = node.Next;

			if ( next == null )
				break;

			// Distance to the edge.
			var edge = next.Position - nodePos;
			var edgeDir = edge.normalized;
			float projLen = Vector2.Dot ( vFromNode, edgeDir );
				
			if ( projLen >= 0 ) {
				float projLenSq = projLen * projLen;

				if ( projLenSq <= edge.sqrMagnitude ) {
					float distToEdgeSq = distFromNodeSq - projLenSq;

					if ( distToEdgeSq < minDistanceSq ) {
						minDistanceSq = distToEdgeSq;
						closestEdgeStart = node;
						closestPos = nodePos + projLen * edgeDir;
					}
				}
			}

			node = next;
		} while ( node != firstNode );

		return	closestEdgeStart;
	}

	private static Color gizmoColor = new Color ( 0.5f, 0, 1 );

	void OnDrawGizmos () {
		if ( Next != null ) {
			DebugHelper.UseGizmos = true;
			DebugHelper.DrawArrow ( Position, Next.Position, gizmoColor );
			DebugHelper.UseGizmos = false;
		}
	}
}
