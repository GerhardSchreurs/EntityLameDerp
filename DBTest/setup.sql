/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

DROP DATABASE IF EXISTS `dbtest`;

CREATE DATABASE IF NOT EXISTS `dbtest` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci */;
USE `dbtest`;

CREATE TABLE IF NOT EXISTS `styles` (
  `style_id` smallint(6) NOT NULL AUTO_INCREMENT,
  `alt_style_id` smallint(6) DEFAULT NULL,
  `parent_style_id` smallint(6) DEFAULT NULL,
  `name` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `weight` tinyint(4) NOT NULL DEFAULT 1,
  PRIMARY KEY (`style_id`),
  UNIQUE KEY `style_id` (`style_id`),
  KEY `rel_styles_alt_style_id` (`alt_style_id`),
  KEY `rel_styles_parent_style_id` (`parent_style_id`),
  CONSTRAINT `rel_styles_alt_style_id` FOREIGN KEY (`alt_style_id`) REFERENCES `styles` (`style_id`),
  CONSTRAINT `rel_styles_parent_style_id` FOREIGN KEY (`parent_style_id`) REFERENCES `styles` (`style_id`)
) ENGINE=InnoDB AUTO_INCREMENT=77 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
ALTER TABLE `styles` AUTO_INCREMENT = 1;

CREATE TABLE IF NOT EXISTS `composers` (
  `composer_id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`composer_id`),
  UNIQUE KEY `composer_id` (`composer_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `scenegroups` (
  `scenegroup_id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`scenegroup_id`),
  UNIQUE KEY `scenegroup_id` (`scenegroup_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `c_scenegroups_composers` (
  `scenegroup_composer_id` int(11) NOT NULL AUTO_INCREMENT,
  `fk_scenegroup_id` int(11) NOT NULL,
  `fk_composer_id` int(11) NOT NULL,
  PRIMARY KEY (`scenegroup_composer_id`),
  UNIQUE KEY `scenegroup_composer_id` (`scenegroup_composer_id`),
  KEY `rel_scenegroups_composers_composer_id` (`fk_composer_id`),
  KEY `rel_scenegroups_composers_scenegroup_id` (`fk_scenegroup_id`),
  CONSTRAINT `rel_scenegroups_composers_composer_id` FOREIGN KEY (`fk_composer_id`) REFERENCES `composers` (`composer_id`),
  CONSTRAINT `rel_scenegroups_composers_scenegroup_id` FOREIGN KEY (`fk_scenegroup_id`) REFERENCES `scenegroups` (`scenegroup_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `trackers` (
  `tracker_id` smallint(6) NOT NULL,
  `name` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `extension` varchar(3) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`tracker_id`),
  UNIQUE KEY `tracker_id` (`tracker_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `tracks` (
  `track_id` int(11) NOT NULL AUTO_INCREMENT,
  `fk_composer_id` int(11) DEFAULT NULL,
  `fk_tracker_id` smallint(6) DEFAULT NULL,
  `fk_style_id` smallint(6) DEFAULT NULL,
  `is_processed` tinyint(1) DEFAULT 0,
  `date_track_created` datetime DEFAULT NULL,
  `date_track_modified` datetime DEFAULT NULL,
  `date_created` datetime DEFAULT current_timestamp(),
  `date_modified` datetime DEFAULT NULL ON UPDATE current_timestamp(),
  `date_processed` datetime DEFAULT NULL,
  `md5` binary(16) DEFAULT NULL,
  `songname` varchar(45) CHARACTER SET utf8 DEFAULT NULL,
  `filename` varchar(45) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `composer` varchar(45) CHARACTER SET utf8 DEFAULT NULL,
  `trackermeta` varchar(45) CHARACTER SET utf8 DEFAULT NULL,
  `speed` varchar(45) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `tempo` varchar(45) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `bpm` varchar(45) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `samplecount` varchar(45) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `sampletextlinecount` varchar(45) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `samples` varchar(45) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `instumentcount` varchar(45) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `instrumenttextlinecount` varchar(45) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `instruments` varchar(45) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `songtext` varchar(45) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`track_id`),
  UNIQUE KEY `track_id` (`track_id`),
  KEY `rel_tracks_composer_id` (`fk_composer_id`),
  KEY `rel_tracks_tracker_id` (`fk_tracker_id`),
  KEY `rel_tracks_style_id` (`fk_style_id`),
  CONSTRAINT `rel_tracks_composer_id` FOREIGN KEY (`fk_composer_id`) REFERENCES `composers` (`composer_id`),
  CONSTRAINT `rel_tracks_style_id` FOREIGN KEY (`fk_style_id`) REFERENCES `styles` (`style_id`),
  CONSTRAINT `rel_tracks_tracker_id` FOREIGN KEY (`fk_tracker_id`) REFERENCES `trackers` (`tracker_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
