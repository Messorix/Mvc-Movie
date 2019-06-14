﻿using Mvc_Movie.Classes;
using MvcMovie.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Mvc_Movie.Controllers
{
    public class MoviesController : Controller
    {
        private DataController dataController = new DataController();

        // GET: Movies
        public ActionResult Index(string movieRating, string movieGenre, string searchString, string sortOrder)
        {
            List<Restriction> restFromAPI = RestTalker.GetRestrictionsFromAPI();

            if (dataController.Restrictions.Count < restFromAPI.Count)
            {
                restFromAPI.RemoveRange(0, dataController.Restrictions.Count);

                foreach (Restriction r in restFromAPI)
                {
                    if (dataController.AddRestriction(r) > 0)
                    {
                        dataController.Restrictions.Add(r);
                    }
                }
            }

            List<Genre> genreFromAPI = RestTalker.GetGenresFromAPI();

            if (dataController.Genres.Count < genreFromAPI.Count)
            {
                genreFromAPI.RemoveRange(0, dataController.Genres.Count);

                foreach (Genre g in genreFromAPI)
                {
                    if (dataController.AddGenre(g) > 0)
                    {
                        dataController.Genres.Add(g);
                    }
                }
            }

            var GenreLst = new List<string>();

            /*var GenreQry = from m in dataController.Movies
                           orderby m.Genre
                           select m.Genre;
            
            GenreLst.AddRange(GenreQry.Distinct());
            ViewBag.movieGenre = new SelectList(GenreLst);*/

            var RatingLst = new List<string>();

            var CertQry = from r in dataController.Restrictions
                          where r.ISO_3166_1 == CultureInfo.CurrentCulture.TwoLetterISOLanguageName
                          select r.Certification;

            RatingLst.AddRange(CertQry.Distinct());
            ViewBag.movieRating = new SelectList(RatingLst);


            var movies = from m in dataController.Movies
                         select m;

            var restrictions = from r in dataController.Restrictions
                               select r;


            List<Movie> x1 = movies.ToList();
            List<Restriction> y1 = restrictions.ToList();




            /*foreach (var movie in movies.ToList())
            {
                foreach (var combi in movRest.Where(x => x.MovieID.Equals(movie.ID)))
                {
                    movie.Restrictions.AddRange(restrictions.Where(y => y.ID.Equals(combi.RestrictionID)));
                }
            }

            /*var groupJoinQuery =
               from movie in movieDB.Movies
               orderby movie.ID
               join rest in movieDB.Restrictions on movie.ID equals rest.ID into prodGroup
               select new
               {
                   Category = category.Name,
                   Products = from prod2 in prodGroup
                              orderby prod2.Name
                              select prod2
               };
            */



            if (!String.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(s => s.Title.Contains(searchString));
            }

            /*if (!string.IsNullOrEmpty(movieGenre))
            {
                movies = movies.Where(x => x.Genre == movieGenre);
            }*/

            if (!string.IsNullOrEmpty(movieRating))
            {
                /*
                movies = movies.Where
                    (x => x.Certifications.Where
                        (y => y.Certification.ISO_3166_1 == CultureInfo.CurrentCulture.TwoLetterISOLanguageName).FirstOrDefault().Certification.Certification == movieRating);
                        */
            }

            ViewBag.TitleSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.PriceSortParm = sortOrder == "Price" ? "price_desc" : "Price";

            switch (sortOrder)
            {
                case "name_desc":
                    movies = movies.OrderByDescending(s => s.Title);
                    break;
                case "Date":
                    movies = movies.OrderBy(s => s.ReleaseDate);
                    break;
                case "date_desc":
                    movies = movies.OrderByDescending(s => s.ReleaseDate);
                    break;
                case "Price":
                    movies = movies.OrderBy(s => s.Price);
                    break;
                case "price_desc":
                    movies = movies.OrderByDescending(s => s.Price);
                    break;
                default:
                    movies = movies.OrderBy(s => s.Title);
                    break;
            }

            return View(movies.ToList());
        }

        // GET: Movies/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Movie movie = dataController.Movies.Single(x => x.ID.Equals(id));

            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // GET: Movies/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Title,ReleaseDate,Genre,Price")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                movie = RestTalker.GetIMDB(movie, dataController.Restrictions, dataController.Genres);
                dataController.AddMovie(movie);

                return RedirectToAction("Index");
            }

            return View(movie);
        }

        // GET: Movies/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = dataController.Movies.Single(x => x.ID.Equals(id));
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Title,Genre,Price")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                //movieDB.Entry(movie).State = EntityState.Modified;
                //movieDB.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = dataController.Movies.Single(x => x.ID.Equals(id));
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Movie movie = dataController.Movies.Single(x => x.ID.Equals(id));
            //dataController.GetData<Movie>("Movies").Remove(movie);
            //movieDB.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //movieDB.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
