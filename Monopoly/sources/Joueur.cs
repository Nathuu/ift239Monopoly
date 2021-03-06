﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using WpfApplication1.sources.Carreaux;
using WpfApplication1.sources.Carreaux.CarreauConcret;

namespace WpfApplication1.sources
{
    public class Joueur
    {
        public Image Image { get; private set; }
        public long Argent { get; set; }
        public Position Position { get; set; } // un objet de type Position
        public string Nom { get; set; }
        public List<CarreauAchetable> Proprietes { get; private set; }
        public List<CarreauAchetable> Trains { get; private set; }
        public List<CarreauAchetable> Services { get; private set; }
        public bool EstPrisonnier { get; set; }
        public bool ACarteSortirPrison { get; set; } // Il y a deux cartes de ce type
        public bool PeutPasserGo { get; set; }
        public int PositionCarreau { get; set; }
        public int CompteurDeDouble { get; set; }
        public bool EstVivant { get; set; }
        private MainWindow mw;

        //Joueur n'a pas de propriétés? Oui il a une liste de proprietes
        public Joueur(String nom, Image image, MainWindow mw)//une piece construite va toujours avoir la meme argent et meme position de depart
        {
            this.Nom = nom;
            this.Image = image;
            this.Argent = 500;
            this.PositionCarreau = 0;
            this.Position = new Position(1, 1);
            this.Image.Width = 20;
            this.Image.Height = 20;
            this.ACarteSortirPrison = false;
            this.EstPrisonnier = false;
            this.PeutPasserGo = true;
            this.Proprietes = new List<CarreauAchetable>();
            this.Trains = new List<CarreauAchetable>();
            this.Services = new List<CarreauAchetable>();
            this.CompteurDeDouble = 0;
            this.mw = mw;
            EstVivant = true;
        }
                
        public int LanceUnDes()// un dé est lancé
        {
            return Plateau.Instance.LanceUnDes();
        }

    public bool Dehypothequer(CarreauAchetable prop)
        {
            if (prop != null)
            {
                if (this.PeutPayer(prop.PrixAchat))
                {
                    if (prop.EstHypotheque)
                    {
                        this.Argent -= prop.PrixAchat;
                        prop.EstHypotheque = false;
                        MessageBox.Show("Propriete: " + prop.positionCarreau + "a ete rachetée!", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Information);
                        return true;
                    }
                    else
                        MessageBox.Show("Propriete: " + prop.positionCarreau + "n'est pas hypothequée!", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Joueur: " + prop.Proprietaire.Nom + "ne peut pas racheter la propriété.", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }
            else
                MessageBox.Show("Propriete: " + prop.positionCarreau + "n'appartient pas au joueur courant", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        internal bool Hypothequer(CarreauAchetable carreauAHypothequer)
        {
            if (carreauAHypothequer != null)
            {
                if (!carreauAHypothequer.EstHypotheque)
                {
                    Depot(carreauAHypothequer.PrixAchat / 2);
                    carreauAHypothequer.EstHypotheque = true;
                    MessageBox.Show("Propriete: " + carreauAHypothequer.positionCarreau + " a ete hypotheque!", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
                else MessageBox.Show("Propriete: " + carreauAHypothequer.positionCarreau + "N'EST PAS HYPOTHEQUE!!!!", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else MessageBox.Show("La propriete selectionnée n'appartient pas au joueur courant", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Information);

            return false;
        }


        // Début du tour du joueur : va appeller LanceDes,Bouger,...
        public void JouerSonTour()
        {
            if (EstPrisonnier)
            {
                TenteSortirPrison();
            }
            else
            {

                int coupDe1 = LanceUnDes();
                int coupDe2 = LanceUnDes();

                int sommeDes = coupDe1 + coupDe2;
                MessageBox.Show("Vous avez eu: (" + coupDe1 + " + " + coupDe2 + ")", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Information);

                if (coupDe1 == coupDe2)
                {
                    CompteurDeDouble += 1;
                    Plateau.Instance.Rejouer = true;
                    if (CompteurDeDouble == 3)
                    {
                        PositionCarreau = 30;
                        Position = Carreau.conversionInt2Position(PositionCarreau);
                        getCarreauActuel().execute();
                        Plateau.Instance.Rejouer = false;
                    }
                    else
                        Avancer(sommeDes);
                }
                else
                {
                    CompteurDeDouble = 0;
                    Avancer(sommeDes);
                    Plateau.Instance.Rejouer = false;
                }
            }
        }


        public void Avancer(int nbCases)
        {
            Debug.Assert(nbCases>0, "Avancer ne devrait etre employer que pour des valeurs positives");
            if (this.EstVivant)
            {
                MessageBox.Show("Joueur " + Nom + " avance de " + nbCases + " cases", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Information);
                int nouvellePosition = (this.PositionCarreau + nbCases) % Plateau.Instance.NombreCarreauxMaximal;

                BougerLaPiece(nouvellePosition);

                if (nouvellePosition < this.PositionCarreau && PeutPasserGo)
                    Depot(Plateau.Instance.MontantCarreauDepart);

                getCarreauActuel().execute();
                mw.MajInformationJoueurs(Plateau.Instance.JoueurCourant);
            }
            else
                    MessageBox.Show("Joueur " + Nom + " ne peut plus avancer puisqu'il est en faillite.", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        public void BougerLaPiece(int nouvellePosition)
        {
            this.PositionCarreau = nouvellePosition;
            this.Position = Carreau.conversionInt2Position(this.PositionCarreau);
            Grid.SetRow(this.Image, this.Position.rangee + 1);
            Grid.SetColumn(this.Image, this.Position.colonne + 1);
        }

     

        /// <summary>
        /// Hypotheque les proprietes jusqu'a etre capable de payer
        /// </summary>
        /// <param name="aPayer"></param>
        /// <returns>Si le joueur peut payer le montant</returns>
        public bool PeutPayer(long aPayer)//on regarde si le joueur peut payer tel montant
        {
            if (Argent > aPayer) return true;
            //for (int index = 0; index < Proprietes.Count; ++index)
            //{
            //    hypothequer(Proprietes[index]);
            //    if (Argent > aPayer) return true;
            //}

            return false;
        }


        /**************************************************************************
         * valeur d'entree : ce que le joueur doit payer
         * valeur Sortie : nouvelle argent de joueur
         * fait la transaction entre deux joueurs
         Cete fonction ne prend pas encore en compte les Avoirs du joueurs(maison, terrain)
         ************************************************************************/
        public long Payer(long aPayer)
        {
            Argent -= aPayer;
            MessageBox.Show("Joueur " + Nom + " paie " + aPayer + "$.\n" +
            "Montant dans le compte: " + Argent + "$", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Information);
            return Argent; // return l'argent  que le jouer a ;
        }
        public long Depot(long deposer)
        {
            Argent += deposer;
            MessageBox.Show("Joueur " + Nom + " dépose " + deposer + "$ dans son compte.\n" +
                "Montant dans le compte: " + Argent + "$", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Information);
            return Argent; // on retourne le nouveau argent
        }



        public bool FaitFaillite()
        {
            MessageBox.Show("Joueur " + Nom + " est GAME OVER!", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Information);
            EstVivant = false;
            Plateau.Instance.Rejouer = false;
            Plateau.Instance.JoueurRestant--;
            GestionnaireJeu.ChangementJoueur();
            return true;
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// retourne un carreau selon la position du jouer.
        /// </returns>
        public Carreau getCarreauActuel()
        {
            return Plateau.Instance.getCarreau(PositionCarreau);
        }


        public bool estSeulProprietaireDeMemeCouleur(CarreauPropriete.Couleurs couleur)
        {
            int nbProprieteCouleur = 0;
            foreach (CarreauPropriete prop in Proprietes)
            {
                if (couleur == prop.Couleur)
                {
                    nbProprieteCouleur++;
                }
            }
            if (couleur == CarreauPropriete.Couleurs.Brun ||
                couleur == CarreauPropriete.Couleurs.BleuFonce)
                return nbProprieteCouleur == 2;
            else
                return nbProprieteCouleur == 3;
        }

        public bool estSeulProprietaireServices()
        {
            int nbServices = 0;
            foreach (CarreauAchetable prop in Proprietes)
            {
                if (prop is CarreauService) ++nbServices;
            }
            return nbServices == 2;
        }

        public void TenteSortirPrison()
        {
            int coupDe1 = LanceUnDes();
            int coupDe2 = LanceUnDes();
            int somme = coupDe1 + coupDe2;
            MessageBox.Show("Joueur " + Nom + " tente de s'échapper.", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Information);

            if (coupDe1 == coupDe2)
            {
                EstPrisonnier = false;
                MessageBox.Show("Joueur " + Nom + " est libre!", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Information);
                Avancer(somme);
            }
            else
                MessageBox.Show("Joueur " + Nom + " resteen prison! (" + coupDe1 + " + " + coupDe2 + ")", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
