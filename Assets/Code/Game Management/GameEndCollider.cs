#pragma strict;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GameEndCollider : MonoBehaviour {
    private List<Bag> bagList = new List<Bag>();
    private List<Player> playerList = new List<Player>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        Collider collider = GetComponent<Collider>();
        collider.isTrigger = true;
        collider.enabled = false;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            Player playerScript = other.gameObject.GetComponent<Player>();
            if (!playerList.Contains(playerScript)) {
                playerList.Add(playerScript);
            }
        } else if (other.CompareTag("Bag")) {
            Bag bagScript = other.gameObject.GetComponent<Bag>();
            if (!bagList.Contains(bagScript)) {
                bagList.Add(bagScript);
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            Player playerScript = other.gameObject.GetComponent<Player>();
            if (!playerList.Contains(playerScript)) {
                playerList.Remove(playerScript);
            }
        } else if (other.CompareTag("Bag")) {
            Bag bagScript = other.gameObject.GetComponent<Bag>();
            if (!bagList.Contains(bagScript)) {
                bagList.Remove(bagScript);
            }
        }
    }

    public int AmountOfPlayers() {
        return playerList.Count;
    }

    ///Returns robber then informant score
    public (float, float) TotalScores() {
        float informantScore = 0;
        float robberScore = 0;

        foreach (var bag in bagList) {
            if (bag.isTampered) {
                informantScore += bag.money;
            } else {
                robberScore += bag.money;
            }
        }

        return (robberScore, informantScore);
    }
}