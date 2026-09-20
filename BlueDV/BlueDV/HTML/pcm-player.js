// Copyright (c) 2018 Samir Das <cse.samir@gmail.com>
// Copyright (c) 2018 David PA7LIM <info@pa7lim.nl>
var canvas;
var analyser;
var canvasCtx;
var visualSelect;
var recordButton, stopButton;


function PCMPlayer(option) {
	this.init(option);
}

PCMPlayer.prototype.init = function (option) {
	var defaults = {
		encoding: '16bitInt',
		channels: 1,
		sampleRate: 8000,
		flushingTime: 1000
	};
	this.option = Object.assign({}, defaults, option);
	this.samples = new Float32Array();
	this.flush = this.flush.bind(this);
	this.interval = setInterval(this.flush, this.option.flushingTime);
	this.maxValue = this.getMaxValue();
	this.typedArray = this.getTypedArray();
	this.createContext();
	david(this);
};


function david(yo) {





}

PCMPlayer.prototype.getMaxValue = function () {
	var encodings = {
		'8bitInt': 128,
		'16bitInt': 32768,
		'32bitInt': 2147483648,
		'32bitFloat': 1
	}

	return encodings[this.option.encoding] ? encodings[this.option.encoding] : encodings['16bitInt'];
};

PCMPlayer.prototype.getTypedArray = function () {
	var typedArrays = {
		'8bitInt': Int8Array,
		'16bitInt': Int16Array,
		'32bitInt': Int32Array,
		'32bitFloat': Float32Array
	}

	return typedArrays[this.option.encoding] ? typedArrays[this.option.encoding] : typedArrays['16bitInt'];
};

PCMPlayer.prototype.createContext = function () {

	var AudioContext = (window.AudioContext || window.webkitAudioContext);
	// OKOK var AudioContext = (window.AudioContext||window.webkitAudioContext||window.mozAudioContext||window.oAudioContext||window.msAudioContext);
	//this.audioCtx = new (window.AudioContext||window.webkitAudioContext||window.mozAudioContext||window.oAudioContext||window.msAudioContext)();

	if (AudioContext) {
		this.audioCtx = new AudioContext();
		//this.audioCtx = new webkitAudioContext();
	} else {
		alert("Browser not supported");
	}
	this.gainNode = this.audioCtx.createGain();
	this.gainNode.gain.value = 1;
	this.gainNode.connect(this.audioCtx.destination);
	this.startTime = this.audioCtx.currentTime;
	this.audioCtx.resume();
	// david
	analyser = this.audioCtx.createAnalyser();
	analyser.minDecibels = -90;
	analyser.maxDecibels = -10;
	analyser.smoothingTimeConstant = 0.85;

	canvas = document.querySelector('.visualizer');
	canvasCtx = canvas.getContext("2d");

	//var intendedWidth = document.querySelector('.wrapper').clientWidth;
	var intendedWidth = 350;
	canvas.setAttribute('width', intendedWidth);
	visualSelect = document.getElementById("visual");
	visualize();

	recordButton = document.getElementById('record');
	stopButton = document.getElementById('stop');

	//recordButton.addEventListener('click', startRecording);
	//stopButton.addEventListener('click', stopRecording);

};

function startRecording() {
	recordButton.disabled = true;
	stopButton.disabled = false;

	//recorder.start();
}


function stopRecording() {
	recordButton.disabled = false;
	stopButton.disabled = true;

	// Stopping the recorder will eventually trigger the `dataavailable` event and we can complete the recording process
	//recorder.stop();
}




PCMPlayer.prototype.isTypedArray = function (data) {
	return (data.byteLength && data.buffer && data.buffer.constructor == ArrayBuffer);
};

PCMPlayer.prototype.feed = function (data) {
	if (!this.isTypedArray(data)) return;
	data = this.getFormatedValue(data);
	var tmp = new Float32Array(this.samples.length + data.length);
	tmp.set(this.samples, 0);
	tmp.set(data, this.samples.length);
	this.samples = tmp;
};

PCMPlayer.prototype.getFormatedValue = function (data) {
	var data = new this.typedArray(data.buffer),
		float32 = new Float32Array(data.length),
		i;

	for (i = 0; i < data.length; i++) {
		float32[i] = data[i] / this.maxValue;
	}
	return float32;
};

PCMPlayer.prototype.volume = function (volume) {
	this.gainNode.gain.value = volume;
};

PCMPlayer.prototype.destroy = function () {
	if (this.interval) {
		clearInterval(this.interval);
	}
	this.samples = null;
	this.audioCtx.close();
	this.audioCtx = null;
};

PCMPlayer.prototype.flush = function () {
	if (!this.samples.length) return;
	var bufferSource = this.audioCtx.createBufferSource(),
		length = this.samples.length / this.option.channels,
		audioBuffer = this.audioCtx.createBuffer(this.option.channels, length, this.option.sampleRate),
		audioData,
		channel,
		offset,
		i,
		decrement;

	// for (channel = 0; channel < this.option.channels; channel++) {
	//     offset = channel;
	//     audioData = audioBuffer.getChannelData(channel);
	//     for (i = 0; i < length; i++) {
	//         audioData[i] = this.samples[offset];
	//         offset += this.option.channels;
	//     }
	// }

	for (channel = 0; channel < this.option.channels; channel++) {
		audioData = audioBuffer.getChannelData(channel);
		offset = channel;
		decrement = 50;
		for (i = 0; i < length; i++) {
			audioData[i] = this.samples[offset];
			/* fadein */
			if (i < 50) {
				audioData[i] = (audioData[i] * i) / 50;
			}
			/* fadeout*/
			if (i >= (length - 51)) {
				audioData[i] = (audioData[i] * decrement--) / 50;
			}
			offset += this.option.channels;
		}
	}

	if (this.startTime < this.audioCtx.currentTime) {
		this.startTime = this.audioCtx.currentTime;
	}


	console.log('start vs current ' + this.startTime + ' vs ' + this.audioCtx.currentTime + ' duration: ' + audioBuffer.duration);
	bufferSource.buffer = audioBuffer;
	bufferSource.connect(this.gainNode);
	bufferSource.connect(analyser);
	bufferSource.start(this.startTime);

	this.startTime += audioBuffer.duration;
	this.samples = new Float32Array();
	//visualize();



}


function visualize() {

	WIDTH = canvas.width;
	HEIGHT = canvas.height;


	analyser.fftSize = 2048;
	var bufferLength = analyser.fftSize;
	console.log(bufferLength);
	var dataArray = new Uint8Array(bufferLength);

	canvasCtx.clearRect(0, 0, WIDTH, HEIGHT);

	var draw = function () {

		drawVisual = requestAnimationFrame(draw);

		analyser.getByteTimeDomainData(dataArray); // current waveform david!!

		canvasCtx.fillStyle = 'rgb(0, 0, 0)';
		//canvasCtx.fillStyle = "rgba(255, 255, 255, 0)";
		canvasCtx.fillRect(0, 0, WIDTH, HEIGHT);

		canvasCtx.lineWidth = 2;
		canvasCtx.strokeStyle = 'rgb(255, 0, 0)';
		canvasCtx.beginPath();

		var sliceWidth = WIDTH * 1.0 / bufferLength;
		var x = 0;

		for (var i = 0; i < bufferLength; i++) {

			var v = dataArray[i] / 128.0;
			var y = v * HEIGHT / 2;

			if (i === 0) {
				canvasCtx.moveTo(x, y);
			} else {
				canvasCtx.lineTo(x, y);
			}

			x += sliceWidth;
		}

		canvasCtx.lineTo(canvas.width, canvas.height / 2);
		canvasCtx.stroke();
	};

	draw();
}
