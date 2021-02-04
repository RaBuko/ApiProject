import { Component } from 'react';
import './Main.css';
import Home from './Home'

class Main extends Component {
  render() {
    return (
    <div className="Main">
      <h1>API Client</h1>
      <ul className="header">
        <li><a href="/">Home</a></li>
        <li><a href="/users">Home</a></li>
        <li><a href="/contact">Contact</a></li>
        <li><a href="/login">Login</a></li>
      </ul>
      <div className="content">

      </div> 
    </div>
    )
  }
}

export default Main;
