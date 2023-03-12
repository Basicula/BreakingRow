import styled from 'styled-components'

export default function Infos({ score_bonuses }) {
  return (
    <InfosContainer>
      {Object.keys(score_bonuses).map((score_bonus, i) => {
        return (
          <InfoContainer key={i}>
            <InfoConditionContainer>x{score_bonus} strike</InfoConditionContainer>
            :
            <InfoResultContainer>x{score_bonuses[score_bonus]} score bonus</InfoResultContainer>
          </InfoContainer>);
      })}
    </InfosContainer>
  );
}

const InfosContainer = styled.div`
  display: flex;
  flex-direction: column;
`;

const InfoContainer = styled.div`
  display: flex;
  flex-direction: row;
`;

const InfoConditionContainer = styled.div`
  margin-right: 5px;
`;
const InfoResultContainer = styled.div`
  margin-left: 5px;
`;

