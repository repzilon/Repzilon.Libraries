exports = {};
exports.RoundOff = (function () {
	function RoundOff() { }
	RoundOff.error = function (value) {
		var power = Math.pow(10, 15 - 2);
		return Math.round(value * power) / power;
	};
	RoundOff.areEqual = function (a, b) {
		return RoundOff.error(a) == RoundOff.error(b);
	};
	return RoundOff;
}());
exports.AminoAcid = (function () {
	function AminoAcid(letter, code, name) {
		if (!letter.match("[A-Za-z]")) {
			throw new Error("letter");
		}
		if ((code == null) || (code.length < 1)) {
			throw new Error("code is null");
		}
		if ((name == null) || (name.length < 1)) {
			throw new Error("name is null");
		}
		if (code.trim().length != 3) {
			throw new Error("An amino acid symbol is made of three letters.");
		}
		this.letter = letter[0];
		this.symbol = code;
		this.name = name.trim();
		this.molarMass = Number.NaN;
		this.pKa1 = Number.NaN;
		this.pKa2 = Number.NaN;
		this.pKaR = Number.NaN;
		this.dicationWhenVeryAcid = false;
		this.formula = null;
	}
	AminoAcid.prototype.clone = function () {
		var copy = new AminoAcid(this.letter, this.symbol, this.name);
		copy.molarMass = this.molarMass;
		copy.pKa1 = this.pKa1;
		copy.pKa2 = this.pKa2;
		copy.pKaR = this.pKaR;
		copy.dicationWhenVeryAcid = this.dicationWhenVeryAcid;
		copy.formula = this.formula;
		return copy;
	};
	AminoAcid.prototype.setPkas = function (pKa1NewValue, pKa2NewValue) {
		return this.setPkasR(pKa1NewValue, pKa2NewValue, Number.NaN, false);
	};
	AminoAcid.prototype.setPkasR = function (pKa1NewValue, pKa2NewValue, pKaRnewValue, isDicationWhenVeryAcid) {
		if (isNaN(pKa1NewValue) || (pKa1NewValue < 1.5) || (pKa1NewValue >= 14)) {
			throw new Error("pKa1NewValue");
		}
		if (isNaN(pKa2NewValue) || (pKa2NewValue < 8) || (pKa2NewValue >= 14)) {
			throw new Error("pKa2NewValue");
		}
		if (!isNaN(pKaRnewValue)) {
			if ((pKaRnewValue < 3) || (pKaRnewValue >= 14)) {
				throw new Error("pKaRnewValue");
			}
		}
		else if (isDicationWhenVeryAcid) {
			throw new Error("An amino acid that is a dication under very acidic conditions must have a pKaR.");
		}
		this.pKa1 = pKa1NewValue;
		this.pKa2 = pKa2NewValue;
		this.pKaR = pKaRnewValue;
		this.dicationWhenVeryAcid = isDicationWhenVeryAcid;
		return this;
	};
	AminoAcid.prototype.setFormula = function (formula) {
		this.formula = formula;
		return this;
	};
	AminoAcid.prototype.isoelectric = function () {
		var ar = this.pKaR;
		var a1 = this.pKa1;
		var a2 = this.pKa2;
		return isNaN(ar) ?
		 AminoAcid.isoelectric(a1, a2) :
		 AminoAcid.isoelectricLateral(a1, a2, this.dicationWhenVeryAcid ? 2 : 1, ar);
	};
	AminoAcid.prototype.weightedCharge = function (pH) {
		if ((pH < 1) || (pH > 14)) {
			throw new Error("pH");
		}
		var dicat = this.dicationWhenVeryAcid;
		var ar = this.pKaR;
		var pkI = this.isoelectric();
		var am;
		if (exports.RoundOff.areEqual(pH, pkI)) {
			return 0;
		} else if (exports.RoundOff.areEqual(pH, this.pKa1)) {
			return dicat ? 1.5 : 0.5;
		} else if (isNaN(ar)) {
			if (exports.RoundOff.areEqual(pH, this.pKa2)) {
				return -0.5;
			} else if ((Math.abs(pH - this.pKa1) <= 1.0) || (Math.abs(pH - this.pKa2) <= 1.0)) {
				if (pH < pkI) {
					return AminoAcid.protonationRatio(pH, this.pKa1);
				} else {
					return AminoAcid.chargeOfLateral(pH, this.pKa2, false, 0 + 1);
				}
			} else {
				return -1 + AminoAcid.protonationRatio(pH, this.pKa1) +
				 AminoAcid.protonationRatio(pH, this.pKa2);
			}
		} else {
			var a2Ltar = this.pKa2 < ar;
			if (exports.RoundOff.areEqual(pH, this.pKa2)) {
				return AminoAcid.chargeOfLateralHalf(false, a2Ltar, dicat);
			} else if (exports.RoundOff.areEqual(pH, ar)) {
				return AminoAcid.chargeOfLateralHalf(true, a2Ltar, dicat);
			} else if (exports.RoundOff.areEqual(pH, (dicat ? a2Ltar ? this.pKa1 + this.pKa2 : this.pKa1 + ar : this.pKa2 + ar) * 0.5)) {
				return dicat ? 1 : -1;
			} else if ((Math.abs(pH - this.pKa1) <= 1.0) || (Math.abs(pH - this.pKa2) <= 1.0) || (Math.abs(pH - this.pKaR) <= 1.0)) {
				am = Math.min(this.pKa2, ar);
				var ah = Math.max(this.pKa2, ar);
				if (2 * pH < this.pKa1 + am) {
					return AminoAcid.chargeOfLateral(pH, this.pKa1, dicat, 2);
				} else if (2 * pH < am + ah) {
					return AminoAcid.chargeOfLateral(pH, am, dicat, 1);
				} else {
					return AminoAcid.chargeOfLateral(pH, ah, dicat, 0);
				}
			} else {
				return (dicat ? -1 : -2) + AminoAcid.protonationRatio(pH, this.pKa1) +
				 AminoAcid.protonationRatio(pH, this.pKa2) +
				 AminoAcid.protonationRatio(pH, ar);
			}
		}
	};
	AminoAcid.protonationRatio = function (pH, pKa) {
		var dblPower = Math.pow(10, pKa - pH);
		return dblPower / (dblPower + 1);
	};
	AminoAcid.chargeOfLateral = function (pH, pKa, dicat, mostAcidicCharge) {
		var dblAlkaliRatio = Math.pow(10, pH - pKa);
		var c = dicat ? mostAcidicCharge : mostAcidicCharge - 1;
		return c - (dblAlkaliRatio / (dblAlkaliRatio + 1));
	};
	AminoAcid.chargeOfLateralHalf = function (pHEqualsPkar, pKa2LessThanPkar, dicat) {
		var blnEquals = pHEqualsPkar == pKa2LessThanPkar;
		return dicat ? blnEquals ? -0.5 : 0.5 : blnEquals ? -1.5 : -0.5;
	};
	AminoAcid.isoelectricLateral = function (pKa1, pKa2, cationCountAtPh1AndHalf, pKaR) {
		if (cationCountAtPh1AndHalf > 2) {
			throw new Error("The lateral chain of a amino acid can only form a cation, a dication or no cation at all under very acidic conditions.");
		} else {
			var sum;
			if (cationCountAtPh1AndHalf == 2) {
				sum = pKa2 + pKaR;
			} else if ((cationCountAtPh1AndHalf == 1) && (pKaR < pKa2)) {
				sum = pKa1 + pKaR;
			} else {
				sum = pKa1 + pKa2;
			}
			return exports.RoundOff.error(0.5 * sum);
		}
	};
	AminoAcid.isoelectric = function (pKa1, pKa2) {
		return exports.RoundOff.error(0.5 * (pKa1 + pKa2));
	};
	AminoAcid.alphaList = [
		new AminoAcid('D', "Asp", "Acide aspartique").setPkasR(2.1, 9.8, 3.9, false).setFormula("HOOC-CH<sub>2</sub>-CH-NH<sub>2</sub>-COOH"),
		new AminoAcid('E', "Glu", "Acide glutamique").setPkasR(2.2, 9.7, 4.2, false).setFormula("HOOC-(CH<sub>2</sub>)<sub>2</sub>-CH-NH<sub>2</sub>-COOH"),
		new AminoAcid('A', "Ala", "Alanine").setPkas(2.3, 9.9).setFormula("CH<sub>3</sub>-CH-NH<sub>2</sub>-COOH"),
		new AminoAcid('R', "Arg", "Arginine").setPkasR(2.2, 9.0, 12.5, true).setFormula("(H<sub>2</sub>N)<sub>2</sub>-C-NH-(CH<sub>2</sub>)<sub>3</sub>-CH-NH<sub>2</sub>-COOH"),
		new AminoAcid('N', "Asn", "Asparagine").setPkas(2.0, 8.8).setFormula("H<sub>2</sub>N-C=O-CH<sub>2</sub>-CH-NH<sub>2</sub>-COOH"),
		new AminoAcid('C', "Cys", "Cystéine").setPkasR(1.7, 10.8, 8.3, false).setFormula("HS-CH<sub>2</sub>-CH-NH<sub>2</sub>-COOH"),
		new AminoAcid('Q', "Gln", "Glutamine").setPkas(2.1, 9.1).setFormula("H<sub>2</sub>N-C=O-(CH<sub>2</sub>)<sub>2</sub>-CH-NH<sub>2</sub>-COOH"),
		new AminoAcid('G', "Gly", "Glycine").setPkas(2.4, 9.7).setFormula("CH<sub>2</sub>-NH<sub>2</sub>-COOH"),
		new AminoAcid('H', "His", "Histidine").setPkasR(1.8, 9.2, 6.0, true).setFormula("C<sub>6</sub>H<sub>9</sub>N<sub>3</sub>O<sub>2</sub>"),
		new AminoAcid('I', "Ile", "Isoleucine").setPkas(2.4, 9.7).setFormula("C<sub>6</sub>H<sub>13</sub>NO<sub>2</sub>"),
		new AminoAcid('L', "Leu", "Leucine").setPkas(2.4, 9.6).setFormula("(H<sub>3</sub>C)<sub>2</sub>-CH-CH<sub>2</sub>-CH-NH<sub>2</sub>-COOH"),
		new AminoAcid('K', "Lys", "Lysine").setPkasR(2.2, 8.9, 10.5, true).setFormula("H<sub>2</sub>N-(CH<sub>2</sub>)<sub>4</sub>-CH-NH<sub>2</sub>-COOH"),
		new AminoAcid('M', "Mét", "Méthionine").setPkas(2.3, 9.2).setFormula("H<sub>3</sub>C-S-(CH<sub>2</sub>)<sub>2</sub>-CH-NH<sub>2</sub>-COOH"),
		new AminoAcid('F', "Phé", "Phénylalanine").setPkas(2.6, 9.2).setFormula("C<sub>9</sub>H<sub>11</sub>NO<sub>2</sub>"),
		new AminoAcid('P', "Pro", "Proline").setPkas(2.0, 10.6).setFormula("C<sub>5</sub>H<sub>9</sub>NO<sub>2</sub>"),
		new AminoAcid('S', "Sér", "Sérine").setPkas(2.2, 9.2).setFormula("HO-CH<sub>2</sub>-CH-NH<sub>2</sub>-COOH"),
		new AminoAcid('T', "Thr", "Thréonine").setPkas(2.6, 10.4).setFormula("H<sub>3</sub>C-OH-CH-CH-NH<sub>2</sub>-COOH"),
		new AminoAcid('W', "Trp", "Tryptophane").setPkas(2.4, 9.4).setFormula("C<sub>11</sub>H<sub>12</sub>N<sub>2</sub>O<sub>2</sub>"),
		new AminoAcid('Y', "Tyr", "Tyrosine").setPkasR(2.2, 9.1, 10.0, false).setFormula("C<sub>9</sub>H<sub>11</sub>NO<sub>3</sub>"),
		new AminoAcid('V', "Val", "Valine").setPkas(2.3, 9.7).setFormula("(H<sub>3</sub>C)<sub>2</sub>-CH-CH-NH<sub>2</sub>-COOH")
	];
	return AminoAcid;
}());
exports.AminoLateralCharge = (function () {
	function AminoLateralCharge() { }
	AminoLateralCharge.demo = function () {
		var colors = ["#36A2EB", "#FF6384", "#4BC0C0", "#FF9F40", "#9966FF", "#FFCD56", "#C9CBCF"];
		Chart.defaults.backgroundColor = '#fff';
		Chart.defaults.borderColor = '#000';
		Chart.defaults.color = '#000';
		var dataSets = [];
		var r = 0;
		for (var a = 0; a < exports.AminoAcid.alphaList.length; a++) {
			if (!isNaN(exports.AminoAcid.alphaList[a].pKaR)) {
				var newDataSet = { label: exports.AminoAcid.alphaList[a].symbol, backgroundColor: colors[r], data: [] };
				for (var f = 100; f <= 1400; f += 5) {
					var pH = exports.RoundOff.error(f * 0.01);
					var q = exports.AminoAcid.alphaList[a].weightedCharge(pH);
					if (!isNaN(q)) {
						newDataSet.data.push({ x: pH, y: q });
					}
				}
				dataSets.push(newDataSet);
				r++;
			}
		}
		new Chart(document.getElementById('myChart'), {
			type: 'scatter',
			data: { datasets: dataSets }
		});
	};
	return AminoLateralCharge;
}());
