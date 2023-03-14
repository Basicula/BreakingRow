import { useState, useEffect } from "react";
import styled from 'styled-components'

import GameField from "./GameField.js";
import Statistics from "./Statistics.js";
import Infos from "./Infos.js";

export default function App() {
  const [window_size, set_window_size] = useState([]);
  const [score, set_score] = useState(0);
  const [moves_count, set_moves_count] = useState(0);
  const [elements_count, set_elements_count] = useState({});
  const [strikes_statistics, set_strike_statistics] = useState({});

  const score_bonuses = {
    3: 1,
    4: 2,
    5: 5,
    6: 5,
    7: 10
  };

  const update_statistics = (value, count) => {
    set_score(score + value * count * score_bonuses[count]);
    var new_elements_count = elements_count;
    if (value in elements_count)
      new_elements_count[value] += count;
    else
      new_elements_count[value] = count;
    set_elements_count(new_elements_count);
    var new_strike_statistics = strikes_statistics;
    if (count in strikes_statistics)
      ++new_strike_statistics[count]
    else
      new_strike_statistics[count] = 1
    set_strike_statistics(new_strike_statistics);
  }


  useEffect(() => {
    function updateSize() {
      set_window_size([window.innerWidth, window.innerHeight]);
    }
    window.addEventListener('resize', updateSize);
    updateSize();
    return () => window.removeEventListener('resize', updateSize);
  }, []);

  return (
    <AppContainer>
      <Statistics elements_count={elements_count} strikes_statistics={strikes_statistics}/>
      <GameFieldContainer>
        <ScoreContainer>
          <ScoreTitleContainer>Score</ScoreTitleContainer>
          <ScoreValueContainer>{score}</ScoreValueContainer>
          <MovesCountTitleContainer>Moves count</MovesCountTitleContainer>
          <MovesCountValueContainer>{moves_count}</MovesCountValueContainer>
        </ScoreContainer>
        <GameField
          width={7}
          height={7}
          onStrike={update_statistics}
          onMovesCountChange={count=>set_moves_count(count)}
        />
      </GameFieldContainer>
      <Infos score_bonuses={score_bonuses}></Infos>
    </AppContainer>
  );
}

const AppContainer = styled.div`
  width: 100%;
  height: 100%;

  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: center;
`;

const ScoreContainer = styled.div`
  width: 100%;

  display: flex;
  flex-direction: row;
  justify-content: space-between;
  padding: 5px;
  margin-bottom: 2px;
  box-sizing: border-box;

  background: #aaa;

  font-size: 48px;

  border-radius: 10px;
`;

const ScoreTitleContainer = styled.div`
  font-weight: bold;
  text-shadow: 2px 2px 5px white;
`;
const ScoreValueContainer = styled.div`
  font-weight: bold;
  text-shadow: 2px 2px 5px white;
`;

const MovesCountTitleContainer = styled.div`
  font-weight: bold;
  text-shadow: 2px 2px 5px white;
`;
const MovesCountValueContainer = styled.div`
  font-weight: bold;
  text-shadow: 2px 2px 5px white;
`;

const GameFieldContainer = styled.div`
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 5px; 
`;
