using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider))]
public class GameEndCollider : MonoBehaviour {
    private List<Bag> bagList = new List<Bag>();
    private List<Player> playerList = new List<Player>();
    public Collider MyCollider { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        MyCollider = GetComponent<Collider>();
        MyCollider.isTrigger = true;
        MyCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("Enter: " + other.gameObject.name);
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
        Debug.Log("Exit: " + other.gameObject.name);
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

    /// <summary>
    /// Gets the amount of players in the collider
    /// </summary>
    /// <returns>integer count</returns>
    public int AmountOfPlayers() {
        return playerList.Count;
    }

    /// <summary>
    /// Returns the count of isInformant = check
    /// </summary>
    /// <param name="isInformant"> wether or not the player should count if its an informant</param>
    /// <returns></returns>
    public int AmountOfPlayers(bool isInformant) {
        int count = 0;
        foreach (var player in playerList) {
            if (player._myNpm.isInformant == isInformant) {
                count++;
            }
        }

        return count;
    }

    ///Returns robber then informant score
    public (int, int) TotalScores() {
        int informantScore = 0;
        int robberScore = 0;

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