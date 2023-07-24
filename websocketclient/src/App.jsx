import React from 'react';
import {DataTable} from "./DataTable";

export default function App() {

  const webSocket = new WebSocket("ws://localhost:5000");

  webSocket.onopen = () => {
    webSocket.send("Websocket connected.");
    console.log("Websocket connected.");
  };
  
  webSocket.onclose = () => {
    webSocket.send("Websocket disconnected.");
    console.log("Websocket disconnected.");
  };

  webSocket.onerror = () => {
    webSocket.send("Websocket error");
    console.log("Websocket error");
  };

  window.onbeforeunload = (ev) => {
    webSocket.close();
    console.log("Websocket disconnected.");
    ev.preventDefault();
  };

  return (
    <>
      <h1> </h1>
      <div className="container">
        <div className="row min-vh-100">
          <div className="col d-flex flex-column justify-content-center align-items-center">
            <h1>thekey - Academy Blog Word Count Map Table</h1>
            <DataTable webSocket = {webSocket} />
          </div>
        </div>
      </div>
    </>
  );
}
